import { Component, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatBadgeModule } from '@angular/material/badge';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { HttpClient, HttpEventType } from '@angular/common/http';
import { ManualMappingComponent } from './manual-mapping.component';
import { DuplicateWarningDialogComponent } from './duplicate-warning-dialog.component';
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

interface DuplicateTransactionDto {
  transactionId: string;
  date: string;
  payee: string;
  amount: number;
  category: string;
  account: string;
}

interface CategorySuggestionDto {
  transactionIndex: number;
  suggestedCategoryId: string;
  categoryName: string;
  confidence: number;
  matchedPattern: string;
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
  duplicates: DuplicateTransactionDto[];
  suggestions: CategorySuggestionDto[];
}

interface ImportTransactionDto {
  date: string;
  payee: string;
  amount: number;
  category: string;
  account: string;
  memo?: string;
  isDuplicate: boolean;
}

interface ConfirmImportCommand {
  transactions: ImportTransactionDto[];
  csvImportId: string;
  fileName: string;
}

interface ConfirmImportResponse {
  success: boolean;
  transactionsImported: number;
  duplicatesSkipped: number;
  transactionIds: string[];
  errorMessage?: string;
}

@Component({
  selector: 'app-import-csv',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatIconModule,
    MatTooltipModule,
    MatChipsModule,
    MatSelectModule,
    MatBadgeModule,
    MatInputModule,
    MatFormFieldModule,
    MatSnackBarModule,
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

  // Duplicate detection state
  duplicateDecisions = signal<Map<number, 'skip' | 'import'>>(new Map());

  // Category suggestions state
  categorySuggestions = signal<Map<number, CategorySuggestionDto>>(new Map());
  selectedCategories = signal<Map<number, string>>(new Map());

  // Story 2.6: Import confirmation state
  showConfirmationPreview = signal(false);
  editedPayees = signal<Map<number, string>>(new Map());
  editedCategories = signal<Map<number, string>>(new Map());
  importing = signal(false);
  availableCategories = signal<Array<{id: string, name: string}>>([
    {id: 'expenses:groceries', name: 'Groceries'},
    {id: 'expenses:shopping', name: 'Shopping'},
    {id: 'expenses:utilities', name: 'Utilities'},
    {id: 'expenses:entertainment', name: 'Entertainment'},
    {id: 'income:salary', name: 'Salary'},
    {id: 'assets:checking', name: 'Checking Account'}
  ]);

  // Computed values
  hasDuplicates = computed(() =>
    (this.previewData()?.duplicates?.length ?? 0) > 0
  );

  duplicatesSkipped = computed(() => {
    let count = 0;
    this.duplicateDecisions().forEach(decision => {
      if (decision === 'skip') count++;
    });
    return count;
  });

  transactionsWithSuggestions = computed(() =>
    this.categorySuggestions().size
  );

  importSummary = computed(() => {
    const preview = this.previewData();
    if (!preview) return null;

    const total = preview.totalRowCount;
    const skipped = this.duplicatesSkipped();
    const withSuggestions = this.transactionsWithSuggestions();
    const needsCategorization = this.transactionsNeedingCategories();

    return {
      total,
      skipped,
      withSuggestions,
      needsCategorization,
      readyToImport: total - skipped - needsCategorization
    };
  });

  // Story 2.6: Computed values for confirmation preview
  transactionsNeedingCategories = computed(() => {
    const preview = this.previewData();
    if (!preview) return 0;

    let count = 0;
    preview.sampleRows.forEach((row, index) => {
      const suggestion = this.categorySuggestions().get(index);
      const editedCategory = this.editedCategories().get(index);
      const selectedCategory = this.selectedCategories().get(index);

      if (!suggestion && !editedCategory && !selectedCategory) {
        count++;
      }
    });
    return count;
  });

  isImportValid = computed(() => {
    return this.transactionsNeedingCategories() === 0;
  });

  constructor(
    private http: HttpClient,
    private apiConfig: ApiConfigService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private router: Router
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

  getDisplayedColumnsWithCategory(): string[] {
    return [...this.getDisplayedColumns(), 'category'];
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
    this.showManualMapping.set(false);
    // After column mapping, trigger duplicate detection
    this.checkForDuplicates();
  }

  backToPreview(): void {
    this.showManualMapping.set(false);
  }

  // Duplicate detection workflow
  checkForDuplicates(): void {
    const preview = this.previewData();
    if (!preview || !preview.duplicates || preview.duplicates.length === 0) {
      // No duplicates, proceed to category suggestions
      this.loadCategorySuggestions();
      return;
    }

    // Open duplicate warning dialog
    const dialogRef = this.dialog.open(DuplicateWarningDialogComponent, {
      width: '600px',
      disableClose: true,
      data: {
        duplicates: preview.duplicates,
        parsedTransactions: preview.sampleRows
      }
    });

    dialogRef.afterClosed().subscribe(decisions => {
      if (decisions) {
        // Store user decisions
        const decisionMap = new Map<number, 'skip' | 'import'>();
        decisions.forEach((decision: { transactionIndex: number; action: 'skip' | 'import' }) => {
          decisionMap.set(decision.transactionIndex, decision.action);
        });
        this.duplicateDecisions.set(decisionMap);

        // After duplicate review, load category suggestions
        this.loadCategorySuggestions();
      }
    });
  }

  // Category suggestions workflow
  loadCategorySuggestions(): void {
    const preview = this.previewData();
    if (!preview || !preview.suggestions) return;

    // Store suggestions in a map by transaction index
    const suggestionMap = new Map<number, CategorySuggestionDto>();
    preview.suggestions.forEach(suggestion => {
      suggestionMap.set(suggestion.transactionIndex, suggestion);
    });
    this.categorySuggestions.set(suggestionMap);
  }

  getSuggestionForTransaction(index: number): CategorySuggestionDto | null {
    return this.categorySuggestions().get(index) || null;
  }

  getSuggestionConfidenceClass(confidence: number): string {
    if (confidence >= 0.8) return 'confidence-high';
    if (confidence >= 0.6) return 'confidence-medium';
    return 'confidence-low';
  }

  acceptSuggestion(transactionIndex: number, suggestion: CategorySuggestionDto): void {
    // Store the accepted category
    this.selectedCategories.update(map => {
      const newMap = new Map(map);
      newMap.set(transactionIndex, suggestion.suggestedCategoryId);
      return newMap;
    });

    // Call API to update ImportRule confidence
    this.http.post(this.apiConfig.getApiUrl('/api/categorize/accept'), {
      ruleId: suggestion.suggestedCategoryId,
      transactionPayee: suggestion.matchedPattern
    }).subscribe({
      next: () => {
        // Success - confidence updated
      },
      error: (error) => {
        console.error('Failed to accept suggestion:', error);
      }
    });
  }

  overrideSuggestion(transactionIndex: number, newCategoryId: string, payeePattern: string): void {
    // Store the overridden category
    this.selectedCategories.update(map => {
      const newMap = new Map(map);
      newMap.set(transactionIndex, newCategoryId);
      return newMap;
    });

    // Call API to create new ImportRule
    this.http.post(this.apiConfig.getApiUrl('/api/categorize/create-rule'), {
      payeePattern,
      categoryId: newCategoryId,
      matchType: 'Contains'
    }).subscribe({
      next: () => {
        // Success - new rule created
      },
      error: (error) => {
        console.error('Failed to create rule:', error);
      }
    });
  }

  canFinalizeImport(): boolean {
    const preview = this.previewData();
    if (!preview) return false;

    // All duplicates must be reviewed
    if (this.hasDuplicates() && this.duplicateDecisions().size === 0) {
      return false;
    }

    // All required fields must be detected
    if (!this.canProceedToNextStep()) {
      return false;
    }

    return true;
  }

  finalizeImport(): void {
    // Story 2.6: Show confirmation preview instead of importing immediately
    this.showConfirmationPreview.set(true);
  }

  // Story 2.6: Handle payee edits in confirmation preview
  onPayeeEdit(index: number, newPayee: string): void {
    this.editedPayees.update(map => {
      const newMap = new Map(map);
      newMap.set(index, newPayee);
      return newMap;
    });
  }

  // Story 2.6: Handle category edits in confirmation preview
  onCategoryEdit(index: number, newCategoryId: string): void {
    this.editedCategories.update(map => {
      const newMap = new Map(map);
      newMap.set(index, newCategoryId);
      return newMap;
    });
  }

  // Story 2.6: Get current payee (edited or original)
  getCurrentPayee(index: number, originalPayee: string): string {
    return this.editedPayees().get(index) || originalPayee;
  }

  // Story 2.6: Get current category (edited, selected, or suggested)
  getCurrentCategory(index: number): string {
    const edited = this.editedCategories().get(index);
    if (edited) return edited;

    const selected = this.selectedCategories().get(index);
    if (selected) return selected;

    const suggestion = this.categorySuggestions().get(index);
    if (suggestion) return suggestion.suggestedCategoryId;

    return '';
  }

  // Story 2.6: Check if transaction is duplicate
  isTransactionDuplicate(index: number): boolean {
    return this.duplicateDecisions().get(index) === 'skip';
  }

  // Story 2.6: Confirm and execute import
  confirmImport(): void {
    const preview = this.previewData();
    const file = this.selectedFile();
    if (!preview || !file) return;

    // Build transaction list from preview data
    const transactions: ImportTransactionDto[] = [];
    preview.sampleRows.forEach((row, index) => {
      const isDuplicate = this.isTransactionDuplicate(index);

      transactions.push({
        date: row['date'] || row['Date'] || '',
        payee: this.getCurrentPayee(index, row['payee'] || row['Payee'] || ''),
        amount: parseFloat(row['amount'] || row['Amount'] || '0'),
        category: this.getCurrentCategory(index),
        account: row['account'] || 'Assets:Checking',
        memo: row['memo'] || row['description'] || undefined,
        isDuplicate
      });
    });

    const command: ConfirmImportCommand = {
      transactions,
      csvImportId: crypto.randomUUID(),
      fileName: file.name
    };

    this.importing.set(true);

    this.http.post<ConfirmImportResponse>(
      this.apiConfig.getApiUrl('/api/import/confirm'),
      command
    ).subscribe({
      next: (response) => {
        this.importing.set(false);

        if (response.success) {
          // Show success snackbar with navigation
          const snackBarRef = this.snackBar.open(
            `Imported ${response.transactionsImported} transactions successfully`,
            'View Dashboard',
            {
              duration: 10000,
              horizontalPosition: 'center',
              verticalPosition: 'bottom',
              panelClass: ['success-snackbar']
            }
          );

          snackBarRef.onAction().subscribe(() => {
            this.router.navigate(['/dashboard']);
          });

          // Reset component state
          this.resetImportState();
        } else {
          this.showErrorSnackbar(response.errorMessage || 'Import failed');
        }
      },
      error: (error) => {
        this.importing.set(false);

        let errorMessage = 'Import failed. Please try again.';
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.status === 0) {
          errorMessage = 'Cannot connect to server. Please ensure the API is running.';
        }

        this.showErrorSnackbar(errorMessage);
      }
    });
  }

  // Story 2.6: Show error snackbar with retry option
  private showErrorSnackbar(message: string): void {
    this.snackBar.open(message, 'Retry', {
      duration: 10000,
      horizontalPosition: 'center',
      verticalPosition: 'bottom',
      panelClass: ['error-snackbar']
    }).onAction().subscribe(() => {
      this.confirmImport();
    });
  }

  // Story 2.6: Reset import state after successful import
  private resetImportState(): void {
    this.selectedFile.set(null);
    this.previewData.set(null);
    this.showConfirmationPreview.set(false);
    this.editedPayees.set(new Map());
    this.editedCategories.set(new Map());
    this.duplicateDecisions.set(new Map());
    this.categorySuggestions.set(new Map());
    this.selectedCategories.set(new Map());
  }

  // Story 2.6: Back to preview from confirmation
  backToPreviewFromConfirmation(): void {
    this.showConfirmationPreview.set(false);
  }
}
