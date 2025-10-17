import { Component, signal, computed, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DragDropModule, CdkDragDrop, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { ApiConfigService } from '../../core/services/api-config.service';

interface ColumnMapping {
  date?: string;
  amount?: string;
  description?: string;
  memo?: string;
  balance?: string;
  account?: string;
}

interface SaveMappingResponse {
  id: string;
  message: string;
}

@Component({
  selector: 'app-manual-mapping',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatChipsModule,
    MatTableModule,
    MatIconModule,
    MatTooltipModule,
    MatInputModule,
    MatFormFieldModule,
    MatSnackBarModule,
    DragDropModule,
    FormsModule
  ],
  templateUrl: './manual-mapping.component.html',
  styleUrls: ['./manual-mapping.component.scss']
})
export class ManualMappingComponent {
  // Inputs
  availableHeaders = input.required<string[]>();
  sampleRows = input.required<Record<string, string>[]>();
  savedMapping = input<Record<string, string> | null>(null);

  // Outputs
  mappingComplete = output<Record<string, string>>();

  // State
  unmappedHeaders = signal<string[]>([]);
  mappings = signal<ColumnMapping>({});
  bankIdentifier = signal<string>('');
  savingMapping = signal(false);

  // Computed
  requiredFieldsMapped = computed(() => {
    const m = this.mappings();
    return !!(m.date && m.amount && m.description);
  });

  validationErrors = computed(() => {
    const errors: string[] = [];
    const m = this.mappings();

    if (!m.date) errors.push('Date column is required');
    if (!m.amount) errors.push('Amount column is required');
    if (!m.description) errors.push('Description column is required');

    return errors;
  });

  constructor(
    private http: HttpClient,
    private snackBar: MatSnackBar,
    private apiConfig: ApiConfigService
  ) {
    // Component initialization - inputs will be available in ngOnInit
  }

  ngOnInit(): void {
    // Initialize mappings after inputs are set by Angular
    this.initializeMappings();
  }

  private initializeMappings(): void {
    const headers = this.availableHeaders();
    const saved = this.savedMapping();

    if (saved) {
      // Pre-fill from saved mapping
      const newMappings: ColumnMapping = {};
      const mapped = new Set<string>();

      Object.entries(saved).forEach(([header, fieldType]) => {
        if (headers.includes(header)) {
          (newMappings as any)[fieldType] = header;
          mapped.add(header);
        }
      });

      this.mappings.set(newMappings);
      this.unmappedHeaders.set(headers.filter(h => !mapped.has(h)));
    } else {
      // All headers unmapped initially
      this.unmappedHeaders.set([...headers]);
      this.mappings.set({});
    }
  }

  onDrop(event: CdkDragDrop<string[]>, fieldType: keyof ColumnMapping): void {
    const header = event.item.data as string;

    // Remove from previous mapping if exists
    const currentMappings = this.mappings();
    const previousField = Object.entries(currentMappings).find(([_, h]) => h === header)?.[0] as keyof ColumnMapping | undefined;

    if (previousField) {
      this.mappings.update(m => {
        const updated = { ...m };
        delete updated[previousField];
        return updated;
      });
    }

    // Remove from unmapped headers
    this.unmappedHeaders.update(headers => headers.filter(h => h !== header));

    // Add to new field mapping
    this.mappings.update(m => ({ ...m, [fieldType]: header }));
  }

  removeMapping(fieldType: keyof ColumnMapping): void {
    const header = this.mappings()[fieldType];
    if (!header) return;

    // Remove from mapping
    this.mappings.update(m => {
      const updated = { ...m };
      delete updated[fieldType];
      return updated;
    });

    // Add back to unmapped headers
    this.unmappedHeaders.update(headers => [...headers, header]);
  }

  getMappedHeader(fieldType: keyof ColumnMapping): string | undefined {
    return this.mappings()[fieldType];
  }

  getPreviewHeaders(): string[] {
    return this.availableHeaders();
  }

  getFieldTypeForHeader(header: string): keyof ColumnMapping | null {
    const mappings = this.mappings();
    const entry = Object.entries(mappings).find(([_, h]) => h === header);
    return entry ? entry[0] as keyof ColumnMapping : null;
  }

  getChipColor(fieldType: keyof ColumnMapping | null): string {
    if (!fieldType) return '';

    const requiredFields: (keyof ColumnMapping)[] = ['date', 'amount', 'description'];
    return requiredFields.includes(fieldType) ? 'primary' : 'accent';
  }

  saveMapping(): void {
    if (!this.requiredFieldsMapped()) {
      this.snackBar.open('Please map all required fields before saving', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    if (!this.bankIdentifier().trim()) {
      this.snackBar.open('Please enter a bank identifier', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    this.savingMapping.set(true);

    // Convert mappings to the format expected by the API (header -> fieldType)
    const columnMappings: Record<string, string> = {};
    Object.entries(this.mappings()).forEach(([fieldType, header]) => {
      if (header) {
        columnMappings[header] = fieldType;
      }
    });

    // Get header signature for exact matching
    const headerSignature = this.availableHeaders();

    const command = {
      bankIdentifier: this.bankIdentifier().trim(),
      columnMappings,
      headerSignature,
      fileNamePattern: '' // Optional for now
    };

    this.http.post<SaveMappingResponse>(this.apiConfig.getApiUrl('/api/import/save-mapping'), command)
      .subscribe({
        next: (response) => {
          this.savingMapping.set(false);
          this.snackBar.open(`Mapping saved for ${this.bankIdentifier()}`, 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
        },
        error: (error) => {
          this.savingMapping.set(false);
          const message = error.error?.message || 'Failed to save mapping';
          this.snackBar.open(message, 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }

  proceedToNext(): void {
    if (!this.requiredFieldsMapped()) return;

    // Convert mappings to the format expected by parent (header -> fieldType)
    const columnMappings: Record<string, string> = {};
    Object.entries(this.mappings()).forEach(([fieldType, header]) => {
      if (header) {
        columnMappings[header] = fieldType;
      }
    });

    this.mappingComplete.emit(columnMappings);
  }

  // Helper for preview table
  getSampleValue(row: Record<string, string>, header: string): string {
    return row[header] || '';
  }

  isRequiredField(fieldType: keyof ColumnMapping | null): boolean {
    if (!fieldType) return false;
    return ['date', 'amount', 'description'].includes(fieldType);
  }
}
