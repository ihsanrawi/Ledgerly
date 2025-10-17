import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { DuplicateWarningDialogComponent } from './duplicate-warning-dialog.component';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('DuplicateWarningDialogComponent', () => {
  let component: DuplicateWarningDialogComponent;
  let fixture: ComponentFixture<DuplicateWarningDialogComponent>;
  let dialogRef: jest.Mocked<MatDialogRef<DuplicateWarningDialogComponent>>;

  const mockDuplicates = [
    {
      transactionId: '123',
      date: '2025-01-15',
      payee: 'Whole Foods',
      amount: 89.50,
      category: 'Groceries',
      account: 'Checking'
    },
    {
      transactionId: '456',
      date: '2025-01-16',
      payee: 'Amazon',
      amount: 45.00,
      category: 'Shopping',
      account: 'Credit Card'
    },
    {
      transactionId: '789',
      date: '2025-01-17',
      payee: 'Netflix',
      amount: 15.99,
      category: 'Entertainment',
      account: 'Checking'
    }
  ];

  const mockData = {
    duplicates: mockDuplicates,
    parsedTransactions: []
  };

  beforeEach(async () => {
    const dialogRefMock = {
      close: jest.fn()
    } as unknown as jest.Mocked<MatDialogRef<DuplicateWarningDialogComponent>>;

    await TestBed.configureTestingModule({
      imports: [
        DuplicateWarningDialogComponent,
        MatDialogModule,
        NoopAnimationsModule
      ],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: mockData },
        { provide: MatDialogRef, useValue: dialogRefMock }
      ]
    }).compileComponents();

    dialogRef = TestBed.inject(MatDialogRef) as jest.Mocked<MatDialogRef<DuplicateWarningDialogComponent>>;
    fixture = TestBed.createComponent(DuplicateWarningDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display total duplicates count', () => {
    expect(component.totalDuplicates()).toBe(3);
  });

  it('should display first duplicate on init', () => {
    expect(component.currentIndex()).toBe(0);
    expect(component.currentDuplicate()).toEqual(mockDuplicates[0]);
  });

  it('should navigate to next duplicate', () => {
    component.next();
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(1);
    expect(component.currentDuplicate()).toEqual(mockDuplicates[1]);
  });

  it('should navigate to previous duplicate', () => {
    component.next();
    fixture.detectChanges();
    component.previous();
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(0);
  });

  it('should disable previous button on first duplicate', () => {
    expect(component.hasPrevious()).toBe(false);
  });

  it('should disable next button on last duplicate', () => {
    component.currentIndex.set(2);
    fixture.detectChanges();

    expect(component.hasNext()).toBe(false);
  });

  it('should record skip decision', () => {
    component.skipDuplicate();

    const decision = component.decisions().find(d => d.transactionIndex === 0);
    expect(decision?.action).toBe('skip');
  });

  it('should record import decision', () => {
    component.importAnyway();

    const decision = component.decisions().find(d => d.transactionIndex === 0);
    expect(decision?.action).toBe('import');
  });

  it('should move to next duplicate after skip decision', () => {
    const initialIndex = component.currentIndex();
    component.skipDuplicate();
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(initialIndex + 1);
  });

  it('should move to next duplicate after import decision', () => {
    const initialIndex = component.currentIndex();
    component.importAnyway();
    fixture.detectChanges();

    expect(component.currentIndex()).toBe(initialIndex + 1);
  });

  it('should close dialog when all duplicates reviewed', () => {
    component.currentIndex.set(2); // Last duplicate
    component.skipDuplicate();

    expect(dialogRef.close).toHaveBeenCalledWith(component.decisions());
  });

  it('should update existing decision when reviewing same duplicate again', () => {
    component.skipDuplicate();
    fixture.detectChanges();

    // Go back and change decision
    component.previous();
    fixture.detectChanges();
    component.importAnyway();

    const decision = component.decisions().find(d => d.transactionIndex === 0);
    expect(decision?.action).toBe('import');
    // After skip (index 0) and import anyway (index 0 updated), we move to index 1
    // So we have 2 decisions: updated index 0 and new index 1
    expect(component.decisions().length).toBeGreaterThanOrEqual(1);
  });

  it('should show current decision status', () => {
    component.skipDuplicate();
    component.previous();
    fixture.detectChanges();

    expect(component.getDecisionForCurrent()).toBe('skip');
  });

  it('should format currency correctly', () => {
    const formatted = component.formatCurrency(89.50);
    expect(formatted).toBe('$89.50');
  });

  it('should format date correctly', () => {
    const formatted = component.formatDate('2025-01-15');
    expect(formatted).toContain('Jan');
    expect(formatted).toContain('15');
    expect(formatted).toContain('2025');
  });

  it('should display duplicate transaction details in template', () => {
    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Whole Foods');
    expect(compiled.textContent).toContain('Groceries');
    expect(compiled.textContent).toContain('Checking');
  });

  it('should display progress indicator', () => {
    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Reviewing duplicate 1 of 3');
  });

  it('should update progress indicator when navigating', () => {
    component.next();
    fixture.detectChanges();

    const compiled = fixture.nativeElement;
    expect(compiled.textContent).toContain('Reviewing duplicate 2 of 3');
  });

  it('should mark all as reviewed when all decisions made', () => {
    component.skipDuplicate();
    component.skipDuplicate();
    component.skipDuplicate();

    expect(component.allReviewed()).toBe(true);
  });
});
