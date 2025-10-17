import { Component, Inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';

interface DuplicateTransactionDto {
  transactionId: string;
  date: string;
  payee: string;
  amount: number;
  category: string;
  account: string;
}

interface DuplicateWarningData {
  duplicates: DuplicateTransactionDto[];
  parsedTransactions: Record<string, string>[];
}

interface DuplicateDecision {
  transactionIndex: number;
  action: 'skip' | 'import';
}

@Component({
  selector: 'app-duplicate-warning-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule
  ],
  templateUrl: './duplicate-warning-dialog.component.html',
  styleUrls: ['./duplicate-warning-dialog.component.scss']
})
export class DuplicateWarningDialogComponent {
  currentIndex = signal(0);
  decisions = signal<DuplicateDecision[]>([]);

  totalDuplicates = computed(() => this.data.duplicates.length);
  currentDuplicate = computed(() => this.data.duplicates[this.currentIndex()]);
  hasNext = computed(() => this.currentIndex() < this.totalDuplicates() - 1);
  hasPrevious = computed(() => this.currentIndex() > 0);
  allReviewed = computed(() =>
    this.decisions().length === this.totalDuplicates()
  );

  constructor(
    public dialogRef: MatDialogRef<DuplicateWarningDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DuplicateWarningData
  ) {}

  skipDuplicate(): void {
    this.recordDecision('skip');
    this.moveToNextOrClose();
  }

  importAnyway(): void {
    this.recordDecision('import');
    this.moveToNextOrClose();
  }

  previous(): void {
    if (this.hasPrevious()) {
      this.currentIndex.update(i => i - 1);
    }
  }

  next(): void {
    if (this.hasNext()) {
      this.currentIndex.update(i => i + 1);
    }
  }

  private recordDecision(action: 'skip' | 'import'): void {
    const index = this.currentIndex();
    const existingDecisionIndex = this.decisions().findIndex(
      d => d.transactionIndex === index
    );

    if (existingDecisionIndex >= 0) {
      // Update existing decision
      const updated = [...this.decisions()];
      updated[existingDecisionIndex] = { transactionIndex: index, action };
      this.decisions.set(updated);
    } else {
      // Add new decision
      this.decisions.update(decisions => [
        ...decisions,
        { transactionIndex: index, action }
      ]);
    }
  }

  private moveToNextOrClose(): void {
    if (this.hasNext()) {
      this.next();
    } else {
      // All duplicates reviewed, close dialog with decisions
      this.dialogRef.close(this.decisions());
    }
  }

  getDecisionForCurrent(): 'skip' | 'import' | null {
    const decision = this.decisions().find(
      d => d.transactionIndex === this.currentIndex()
    );
    return decision?.action || null;
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD'
    }).format(amount);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}
