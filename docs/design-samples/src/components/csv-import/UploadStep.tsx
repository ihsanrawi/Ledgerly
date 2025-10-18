import { useState, useCallback } from "react";
import { Upload, FileText, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { CSVFile } from "./CSVImportDialog";

interface UploadStepProps {
  onComplete: (file: CSVFile) => void;
}

export function UploadStep({ onComplete }: UploadStepProps) {
  const [uploadedFile, setUploadedFile] = useState<File | null>(null);
  const [isDragging, setIsDragging] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const parseCSV = (file: File): Promise<CSVFile> => {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = (e) => {
        try {
          const text = e.target?.result as string;
          const lines = text.split("\n").filter((line) => line.trim());
          const headers = lines[0].split(",").map((h) => h.trim());
          const data = lines.slice(1).map((line) => {
            const values = line.split(",").map((v) => v.trim());
            return headers.reduce((obj, header, index) => {
              obj[header] = values[index];
              return obj;
            }, {} as any);
          });

          resolve({
            name: file.name,
            size: file.size,
            data,
            columns: headers,
          });
        } catch (err) {
          reject(new Error("Failed to parse CSV file"));
        }
      };
      reader.onerror = () => reject(new Error("Failed to read file"));
      reader.readAsText(file);
    });
  };

  const handleFileSelect = useCallback(async (file: File) => {
    setError(null);

    // Validate file
    if (!file.name.endsWith(".csv")) {
      setError("Please upload a CSV file");
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      setError("File size must be less than 10MB");
      return;
    }

    setUploadedFile(file);
  }, []);

  const handleDrop = useCallback(
    (e: React.DragEvent) => {
      e.preventDefault();
      setIsDragging(false);

      const file = e.dataTransfer.files[0];
      if (file) handleFileSelect(file);
    },
    [handleFileSelect]
  );

  const handleDragOver = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  }, []);

  const handleDragLeave = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  }, []);

  const handleFileInput = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (file) handleFileSelect(file);
    },
    [handleFileSelect]
  );

  const handleProceed = async () => {
    if (!uploadedFile) return;

    try {
      const csvFile = await parseCSV(uploadedFile);
      onComplete(csvFile);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to process file");
    }
  };

  return (
    <div className="space-y-6 py-6">
      <div
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        className={`border-2 border-dashed rounded-lg p-12 text-center transition-colors ${
          isDragging ? "border-accent bg-accent/10" : "border-border hover:border-accent/50"
        }`}
      >
        <Upload className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
        <p className="text-lg font-medium mb-2">Drag and drop your CSV file here</p>
        <p className="text-sm text-muted-foreground mb-4">or</p>
        <label htmlFor="file-upload">
          <Button variant="outline" asChild>
            <span className="cursor-pointer">Browse Files</span>
          </Button>
        </label>
        <input id="file-upload" type="file" accept=".csv" className="hidden" onChange={handleFileInput} />
        <p className="text-xs text-muted-foreground mt-4">Supports CSV files up to 10MB</p>
      </div>

      {error && (
        <div className="bg-destructive/10 border border-destructive text-destructive px-4 py-3 rounded-lg">
          {error}
        </div>
      )}

      {uploadedFile && (
        <div className="bg-muted rounded-lg p-4 flex items-center justify-between">
          <div className="flex items-center gap-3">
            <FileText className="h-8 w-8 text-accent" />
            <div>
              <p className="font-medium">{uploadedFile.name}</p>
              <p className="text-sm text-muted-foreground">
                {(uploadedFile.size / 1024).toFixed(2)} KB
              </p>
            </div>
          </div>
          <Button
            variant="ghost"
            size="icon"
            onClick={() => {
              setUploadedFile(null);
              setError(null);
            }}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>
      )}

      <div className="flex justify-end">
        <Button onClick={handleProceed} disabled={!uploadedFile} size="lg">
          Continue to Column Mapping
        </Button>
      </div>
    </div>
  );
}
