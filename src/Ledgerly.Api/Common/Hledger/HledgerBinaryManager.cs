using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Ledgerly.Api.Common.Exceptions;
using Serilog;
using Serilog.Context;

namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Manages embedded hledger binaries across platforms.
/// Handles extraction, permissions, version verification, and platform detection.
/// </summary>
public class HledgerBinaryManager
{
    private readonly ILogger<HledgerBinaryManager> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private static readonly Dictionary<string, string> BinaryChecksums = new()
    {
        ["hledger-windows-x64.exe"] = "3edd8d9fd6ea906335ef368f51c022b90bcb748f1444f30e43c134e5fd821cf8",
        ["hledger-macos-x64"] = "55e9d047347bfb7de6a11c4d0d870e3e99e027219f8714727acf7145b52b4ac5",
        ["hledger-linux-x64"] = "9eb0b8eeabb5c92ad0f4cae4d2c7a11d7eaf56027f5290faf9a3f2cc0b998e38"
    };

    private readonly string _extractionPath;
    private string? _cachedBinaryPath;
    private static readonly SemaphoreSlim ExtractionLock = new(1, 1);

    public HledgerBinaryManager(
        ILogger<HledgerBinaryManager> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _extractionPath = GetExtractionPath();
    }

    /// <summary>
    /// Gets the path to the platform-appropriate hledger executable.
    /// Extracts binary on first run if needed.
    /// </summary>
    public async Task<string> GetHledgerBinaryPath()
    {
        using (LogContext.PushProperty("CorrelationId", GetOrCreateCorrelationId()))
        {
            if (_cachedBinaryPath != null && File.Exists(_cachedBinaryPath))
            {
                return _cachedBinaryPath;
            }

            var binaryName = GetPlatformBinaryName();
            var extractedPath = Path.Combine(_extractionPath, binaryName);

            // Use lock to prevent concurrent extraction/validation attempts
            await ExtractionLock.WaitAsync();
            try
            {
                if (!File.Exists(extractedPath))
                {
                    _logger.LogInformation("hledger binary not found at {Path}, extracting...", extractedPath);
                    await ExtractEmbeddedBinary();
                }

                if (!await ValidateBinary())
                {
                    throw new HledgerBinaryNotFoundException(
                        $"hledger binary validation failed at {extractedPath}");
                }
            }
            finally
            {
                ExtractionLock.Release();
            }

            _cachedBinaryPath = extractedPath;
            _logger.LogInformation("hledger binary ready at {Path}", extractedPath);
            return extractedPath;
        }
    }

    /// <summary>
    /// Validates binary integrity using SHA256 checksum and version execution.
    /// </summary>
    public async Task<bool> ValidateBinary()
    {
        using (LogContext.PushProperty("CorrelationId", GetOrCreateCorrelationId()))
        {
            var binaryName = GetPlatformBinaryName();
            var binaryPath = Path.Combine(_extractionPath, binaryName);

            if (!File.Exists(binaryPath))
            {
                _logger.LogWarning("Binary not found at {Path}", binaryPath);
                return false;
            }

            // Verify SHA256 checksum
            var actualChecksum = await ComputeSha256(binaryPath);
            var expectedChecksum = BinaryChecksums[binaryName];

            if (!actualChecksum.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(
                    "SHA256 checksum mismatch for {Binary}. Expected: {Expected}, Actual: {Actual}",
                    binaryName, expectedChecksum, actualChecksum);
                return false;
            }

            _logger.LogDebug("SHA256 checksum verified for {Binary}", binaryName);

            // Execute --version to verify binary works
            try
            {
                var version = await GetHledgerVersion();
                _logger.LogInformation("hledger version verified: {Version}", version);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute hledger --version");
                return false;
            }
        }
    }

    /// <summary>
    /// Extracts embedded binary from assembly resources.
    /// </summary>
    public async Task ExtractEmbeddedBinary()
    {
        using (LogContext.PushProperty("CorrelationId", GetOrCreateCorrelationId()))
        {
            var binaryName = GetPlatformBinaryName();
            var resourceName = $"Ledgerly.Api.Resources.Binaries.{binaryName}";
            var extractedPath = Path.Combine(_extractionPath, binaryName);

            _logger.LogInformation("Extracting {Resource} to {Path}", resourceName, extractedPath);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream == null)
            {
                var availableResources = assembly.GetManifestResourceNames();
                _logger.LogError(
                    "Embedded resource {Resource} not found. Available resources: {Resources}",
                    resourceName, string.Join(", ", availableResources));
                throw new HledgerBinaryNotFoundException(
                    $"Embedded resource {resourceName} not found in assembly");
            }

            // Create extraction directory
            Directory.CreateDirectory(_extractionPath);

            // Extract binary
            await using var fileStream = File.Create(extractedPath);
            await resourceStream.CopyToAsync(fileStream);

            _logger.LogInformation("Binary extracted to {Path} ({Bytes} bytes)",
                extractedPath, new FileInfo(extractedPath).Length);

            // Set executable permissions on Unix platforms
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                SetExecutablePermissions(extractedPath);
            }
        }
    }

    /// <summary>
    /// Returns hledger version string (e.g., "1.32.3").
    /// </summary>
    public async Task<string> GetHledgerVersion()
    {
        using (LogContext.PushProperty("CorrelationId", GetOrCreateCorrelationId()))
        {
            var binaryPath = Path.Combine(_extractionPath, GetPlatformBinaryName());

            if (!File.Exists(binaryPath))
            {
                throw new HledgerBinaryNotFoundException(
                    $"hledger binary not found at {binaryPath}");
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = binaryPath,
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processStartInfo);
            if (process == null)
            {
                throw new HledgerProcessException(
                    "Failed to start hledger process", -1, "", "Process.Start returned null");
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new HledgerProcessException(
                    "hledger --version failed", process.ExitCode, output, error);
            }

            // Parse version from output like "hledger 1.32.3, linux-x86_64"
            var versionMatch = System.Text.RegularExpressions.Regex.Match(
                output, @"hledger\s+(\d+\.\d+\.\d+)");

            if (!versionMatch.Success)
            {
                throw new HledgerParseException(
                    $"Failed to parse version from output: {output}", output);
            }

            return versionMatch.Groups[1].Value;
        }
    }

    private static string GetPlatformBinaryName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "hledger-windows-x64.exe";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "hledger-macos-x64";
        }
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "hledger-linux-x64";
        }

        throw new PlatformNotSupportedException(
            $"Platform not supported: {RuntimeInformation.OSDescription}");
    }

    private static string GetExtractionPath()
    {
        string basePath;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else
        {
            // Linux/macOS: ~/.local/share/Ledgerly/bin/
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            basePath = Path.Combine(home, ".local", "share");
        }

        return Path.Combine(basePath, "Ledgerly", "bin");
    }

    private void SetExecutablePermissions(string filePath)
    {
        using (LogContext.PushProperty("CorrelationId", GetOrCreateCorrelationId()))
        {
            try
            {
                // chmod +x using Process
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var chmodProcess = Process.Start(processStartInfo);

                if (chmodProcess == null)
                {
                    _logger.LogError("Failed to start chmod process for {FilePath}", filePath);
                    throw new HledgerBinaryNotFoundException(
                        $"Failed to set executable permissions: chmod process did not start");
                }

                chmodProcess.WaitForExit();

                if (chmodProcess.ExitCode != 0)
                {
                    var stderr = chmodProcess.StandardError.ReadToEnd();
                    _logger.LogError(
                        "chmod failed with exit code {ExitCode} for {FilePath}. Error: {Error}",
                        chmodProcess.ExitCode, filePath, stderr);
                    throw new HledgerBinaryNotFoundException(
                        $"Failed to set executable permissions: chmod exited with code {chmodProcess.ExitCode}. {stderr}");
                }

                _logger.LogDebug("Successfully set executable permissions for {FilePath}", filePath);
            }
            catch (Exception ex) when (ex is not HledgerBinaryNotFoundException)
            {
                _logger.LogError(ex, "Exception while setting executable permissions for {FilePath}", filePath);
                throw new HledgerBinaryNotFoundException(
                    $"Failed to set executable permissions for {filePath}", ex);
            }
        }
    }

    private static async Task<string> ComputeSha256(string filePath)
    {
        using var sha256 = SHA256.Create();
        await using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var hashBytes = await sha256.ComputeHashAsync(fileStream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    private string GetOrCreateCorrelationId()
    {
        // Try to get correlation ID from HTTP context
        var correlationId = _httpContextAccessor?.HttpContext?.Request.Headers["X-Correlation-ID"].FirstOrDefault();

        // If not in HTTP context, generate a new one
        return correlationId ?? Guid.NewGuid().ToString();
    }
}
