import { Component, Inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';

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
    MatCardModule,
    MatChipsModule
  ],
  templateUrl: './duplicate-warning-dialog.component.html',
  styleUrls: ['./duplicate-warning-dialog.component.scss']
})
export class DuplicateWarningDialogComponent {
  currentIndex = signal(0);
  decisions = signal<DuplicateDecision[]>([]);

  // Computed properties
  totalDuplicates = computed(() => this.data.duplicates.length);
  currentDuplicate = computed(() => this.data.duplicates[this.currentIndex()]);
  currentNew = computed(() => this.data.parsedTransactions[this.currentIndex()]);
  hasNext = computed(() => this.currentIndex() < this.totalDuplicates() - 1);
  hasPrevious = computed(() => this.currentIndex() > 0);
  allReviewed = computed(() =>
    this.decisions().length === this.totalDuplicates()
  );

  // Computed: Get differences between existing and new transaction
  differences = computed(() => {
    const existing = this.currentDuplicate();
    const newTxn = this.currentNew();
    const diffs: string[] = [];

    if (!existing || !newTxn) return diffs;

    // Compare date
    const existingDate = existing.date;
    const newDate = newTxn['date'] || newTxn['Date'] || '';
    if (existingDate !== newDate) diffs.push('date');

    // Compare payee
    const existingPayee = existing.payee;
    const newPayee = newTxn['payee'] || newTxn['Payee'] || '';
    if (existingPayee !== newPayee) diffs.push('payee');

    // Compare amount
    const existingAmount = existing.amount;
    const newAmount = parseFloat(newTxn['amount'] || newTxn['Amount'] || '0');
    if (Math.abs(existingAmount - newAmount) > 0.01) diffs.push('amount');

    // Compare category
    const existingCategory = existing.category;
    const newCategory = newTxn['category'] || newTxn['Category'] || '';
    if (existingCategory !== newCategory) diffs.push('category');

    // Compare account
    const existingAccount = existing.account;
    const newAccount = newTxn['account'] || newTxn['Account'] || '';
    if (existingAccount !== newAccount) diffs.push('account');

    return diffs;
  });

  // Computed: Calculate match confidence
  matchConfidence = computed(() => {
    const diffs = this.differences();

    // Exact match - no differences
    if (diffs.length === 0) return 'exact';

    // Likely match - only minor differences (category or account)
    if (diffs.length === 1 && (diffs.includes('category') || diffs.includes('account'))) {
      return 'likely';
    }

    // Likely match - date within 1 day, amount exact, payee exact
    if (diffs.length === 1 && diffs.includes('date')) {
      return 'likely';
    }

    // Possible match - more significant differences
    return 'possible';
  });

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

  skipAllRemaining(): void {
    // Mark all remaining duplicates as skip
    for (let i = this.currentIndex(); i < this.totalDuplicates(); i++) {
      this.recordDecisionAtIndex(i, 'skip');
    }
    this.dialogRef.close(this.decisions());
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
    this.recordDecisionAtIndex(this.currentIndex(), action);
  }

  private recordDecisionAtIndex(index: number, action: 'skip' | 'import'): void {
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

  // Helper to check if a field differs
  isDifferent(field: string): boolean {
    return this.differences().includes(field);
  }

  // Get new transaction field value
  getNewValue(field: string): string {
    const newTxn = this.currentNew();
    if (!newTxn) return '';

    // Try both lowercase and capitalized versions
    return newTxn[field] || newTxn[field.charAt(0).toUpperCase() + field.slice(1)] || '';
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

  parseAmount(value: string): number {
    return parseFloat(value) || 0;
  }

  getRemainingCount(): number {
    return this.totalDuplicates() - this.currentIndex();
  }
}
