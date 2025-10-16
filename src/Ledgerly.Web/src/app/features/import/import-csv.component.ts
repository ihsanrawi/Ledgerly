import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { ManualMappingComponent } from './manual-mapping.component';
import { ApiConfigService } from '../../core/services/api-config.service';

interface CsvParseError {
  lineNumber: number;
  errorMessage: string;
  columnName?: string;
}

interface ColumnDetectionResult {
  detectedMappings: Record<string, string>; // header -> field type
  confidenceScores: Record<string, number>; // field type -> confidence
  warnings: string[];
  allRequiredFieldsDetected: boolean;
}

interface PreviewCsvResponse {
  headers: string[];
  sampleRows: Record<string, string>[];
  totalRowCount: number;
  detectedDelimiter: string;
  detectedEncoding: string;
  errors: CsvParseError[];
  columnDetection?: ColumnDetectionResult | null;
  requiresManualMapping: boolean;
  savedMapping?: Record<string, string> | null;
  availableHeaders: string[];
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
    MatIconModule,
    MatTooltipModule,
    MatChipsModule,
    ManualMappingComponent
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
  showManualMapping = signal(false);
  finalColumnMapping = signal<Record<string, string> | null>(null);

  constructor(
    private http: HttpClient,
    private apiConfig: ApiConfigService
  ) {}

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

    this.http.post<PreviewCsvResponse>(this.apiConfig.getApiUrl('/api/import/preview'), formData, {
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

  // Column detection helpers
  getDetectedFieldType(header: string): string | null {
    const detection = this.previewData()?.columnDetection;
    if (!detection) return null;
    return detection.detectedMappings[header] || null;
  }

  getFieldTypeLabel(fieldType: string | null): string {
    if (!fieldType) return '';
    return fieldType.charAt(0).toUpperCase() + fieldType.slice(1);
  }

  getConfidenceIcon(fieldType: string | null): string {
    if (!fieldType) return '';

    const detection = this.previewData()?.columnDetection;
    if (!detection || !detection.confidenceScores[fieldType]) return '';

    const confidence = detection.confidenceScores[fieldType];
    if (confidence >= 0.9) return 'check_circle'; // Green checkmark
    if (confidence >= 0.7) return 'warning'; // Yellow warning
    return 'error'; // Red alert
  }

  getConfidenceColor(fieldType: string | null): string {
    if (!fieldType) return '';

    const detection = this.previewData()?.columnDetection;
    if (!detection || !detection.confidenceScores[fieldType]) return '';

    const confidence = detection.confidenceScores[fieldType];
    if (confidence >= 0.9) return 'success';
    if (confidence >= 0.7) return 'warn';
    return 'error';
  }

  getConfidenceTooltip(header: string): string {
    const fieldType = this.getDetectedFieldType(header);
    if (!fieldType) return 'Not detected';

    const detection = this.previewData()?.columnDetection;
    if (!detection || !detection.confidenceScores[fieldType]) return 'Not detected';

    const confidence = detection.confidenceScores[fieldType];
    return `Detected as ${this.getFieldTypeLabel(fieldType)} (${Math.round(confidence * 100)}% confidence)`;
  }

  hasDetectionWarnings(): boolean {
    const detection = this.previewData()?.columnDetection;
    return detection?.warnings && detection.warnings.length > 0 || false;
  }

  canProceedToNextStep(): boolean {
    const detection = this.previewData()?.columnDetection;
    return detection?.allRequiredFieldsDetected || false;
  }

  requiresManualMapping(): boolean {
    return this.previewData()?.requiresManualMapping || false;
  }

  proceedToManualMapping(): void {
    this.showManualMapping.set(true);
  }

  onMappingComplete(mapping: Record<string, string>): void {
    this.finalColumnMapping.set(mapping);
    // TODO: Proceed to next step in import workflow (duplicate detection/category suggestions)
    console.log('Column mapping complete:', mapping);
  }

  backToPreview(): void {
    this.showManualMapping.set(false);
  }
}
