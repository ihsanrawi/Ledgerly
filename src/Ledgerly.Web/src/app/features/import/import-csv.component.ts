import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient, HttpEventType } from '@angular/common/http';

interface CsvParseError {
  lineNumber: number;
  errorMessage: string;
  columnName?: string;
}

interface PreviewCsvResponse {
  headers: string[];
  sampleRows: Record<string, string>[];
  totalRowCount: number;
  detectedDelimiter: string;
  detectedEncoding: string;
  errors: CsvParseError[];
}

@Component({
  selector: 'app-import-csv',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatIconModule
  ],
  templateUrl: './import-csv.component.html',
  styleUrls: ['./import-csv.component.scss']
})
export class ImportCsvComponent {
  selectedFile = signal<File | null>(null);
  uploading = signal(false);
  previewData = signal<PreviewCsvResponse | null>(null);
  errorMessage = signal<string | null>(null);
  dragOver = signal(false);

  constructor(private http: HttpClient) {}

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      this.handleFileSelection(files[0]);
    }
  }

  onFilePickerChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFileSelection(input.files[0]);
    }
  }

  handleFileSelection(file: File): void {
    // Validate file type
    if (!file.name.toLowerCase().endsWith('.csv')) {
      this.errorMessage.set('Only .csv files are allowed');
      this.selectedFile.set(null);
      return;
    }

    // Validate file size (50MB max)
    const maxSizeBytes = 50 * 1024 * 1024;
    if (file.size > maxSizeBytes) {
      this.errorMessage.set('File size must not exceed 50MB');
      this.selectedFile.set(null);
      return;
    }

    this.selectedFile.set(file);
    this.errorMessage.set(null);
    this.previewData.set(null);
  }

  uploadAndPreview(): void {
    const file = this.selectedFile();
    if (!file) return;

    this.uploading.set(true);
    this.errorMessage.set(null);

    const formData = new FormData();
    formData.append('file', file);

    this.http.post<PreviewCsvResponse>('http://localhost:5000/api/import/preview', formData, {
      reportProgress: true,
      observe: 'events'
    }).subscribe({
      next: (event) => {
        if (event.type === HttpEventType.Response) {
          this.uploading.set(false);
          this.previewData.set(event.body);

          // Check for parse errors
          if (event.body?.errors && event.body.errors.length > 0) {
            const errorSummary = event.body.errors
              .map(e => `Line ${e.lineNumber}: ${e.errorMessage}`)
              .join(', ');
            this.errorMessage.set(`Parse warnings: ${errorSummary}`);
          }
        }
      },
      error: (error) => {
        this.uploading.set(false);

        if (error.error?.message) {
          this.errorMessage.set(error.error.message);
        } else if (error.status === 0) {
          this.errorMessage.set('Cannot connect to server. Please ensure the API is running.');
        } else {
          this.errorMessage.set('Failed to parse CSV file. Please check the file format.');
        }
      }
    });
  }

  removeFile(): void {
    this.selectedFile.set(null);
    this.previewData.set(null);
    this.errorMessage.set(null);
  }

  getDisplayedColumns(): string[] {
    const preview = this.previewData();
    return preview?.headers || [];
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  }
}
