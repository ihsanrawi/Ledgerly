import { Component, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { HttpClient } from '@angular/common/http';
import { ApiConfigService } from '../../core/services/api-config.service';

interface SavedMappingDto {
  id: string;
  bankIdentifier: string;
  columnMappings: Record<string, string>;
  createdAt: string;
  lastUsedAt: string;
  timesUsed: number;
}

@Component({
  selector: 'app-import-rules',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatIconModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './import-rules.component.html',
  styleUrls: ['./import-rules.component.scss']
})
export class ImportRulesComponent implements OnInit {
  savedMappings = signal<SavedMappingDto[]>([]);
  loading = signal(false);
  displayedColumns = ['bankIdentifier', 'mappedColumns', 'timesUsed', 'lastUsedAt', 'actions'];

  constructor(
    private http: HttpClient,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private apiConfig: ApiConfigService
  ) {}

  ngOnInit(): void {
    this.loadSavedMappings();
  }

  loadSavedMappings(): void {
    this.loading.set(true);

    this.http.get<SavedMappingDto[]>(this.apiConfig.getApiUrl('/api/import/mappings'))
      .subscribe({
        next: (mappings) => {
          this.loading.set(false);
          this.savedMappings.set(mappings);
        },
        error: (error) => {
          this.loading.set(false);
          const message = error.error?.message || 'Failed to load saved mappings';
          this.snackBar.open(message, 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }

  getMappedColumnsDisplay(columnMappings: Record<string, string>): string {
    const fieldTypes = Object.values(columnMappings);
    const requiredFields = ['date', 'amount', 'description'];
    const optionalFields = fieldTypes.filter(f => !requiredFields.includes(f));

    return `${fieldTypes.length} columns (${requiredFields.length} required${optionalFields.length > 0 ? ', ' + optionalFields.length + ' optional' : ''})`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  viewMapping(mapping: SavedMappingDto): void {
    const fieldTypes = Object.entries(mapping.columnMappings)
      .map(([header, fieldType]) => `  • ${header} → ${fieldType}`)
      .join('\n');

    this.snackBar.open(
      `${mapping.bankIdentifier}\n\nColumn Mappings:\n${fieldTypes}`,
      'Close',
      {
        duration: 10000,
        verticalPosition: 'top',
        panelClass: ['info-snackbar']
      }
    );
  }

  deleteMapping(mapping: SavedMappingDto): void {
    if (!confirm(`Are you sure you want to delete the mapping for "${mapping.bankIdentifier}"?`)) {
      return;
    }

    this.http.delete(this.apiConfig.getApiUrl(`/api/import/mappings/${mapping.id}`))
      .subscribe({
        next: () => {
          this.snackBar.open(`Mapping for ${mapping.bankIdentifier} deleted`, 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          this.loadSavedMappings();
        },
        error: (error) => {
          const message = error.error?.message || 'Failed to delete mapping';
          this.snackBar.open(message, 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar']
          });
        }
      });
  }

  refresh(): void {
    this.loadSavedMappings();
  }
}
