import { Injectable } from '@angular/core';

/**
 * Service providing centralized API configuration.
 *
 * This service replaces hardcoded API URLs throughout the application
 * with a single source of truth that can be easily configured per environment.
 *
 * Story 2.4 - QA Refactoring: Extracted API base URL for maintainability.
 */
@Injectable({
  providedIn: 'root'
})
export class ApiConfigService {
  /**
   * Base URL for the backend API.
   *
   * TODO: For production deployment, this should be configured via:
   * - Environment variables (via environment.ts files)
   * - Build-time configuration replacement
   * - Runtime configuration loaded from assets/config.json
   */
  private readonly apiBaseUrl = 'http://localhost:5000';

  /**
   * Get the full API URL for a given endpoint path.
   *
   * @param path - The API endpoint path (with or without leading slash)
   * @returns The complete API URL
   *
   * @example
   * ```typescript
   * const url = apiConfig.getApiUrl('/api/import/preview');
   * // Returns: 'http://localhost:5000/api/import/preview'
   * ```
   */
  getApiUrl(path: string): string {
    // Ensure path starts with /
    const normalizedPath = path.startsWith('/') ? path : `/${path}`;
    return `${this.apiBaseUrl}${normalizedPath}`;
  }

  /**
   * Get the base API URL without any path.
   *
   * @returns The base API URL
   */
  getBaseUrl(): string {
    return this.apiBaseUrl;
  }
}
