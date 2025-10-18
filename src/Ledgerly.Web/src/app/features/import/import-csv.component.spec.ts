import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ImportCsvComponent } from './import-csv.component';

// Mock crypto.randomUUID for tests
Object.defineProperty(globalThis, 'crypto', {
  value: {
    randomUUID: () => 'test-uuid-12345'
  }
});

describe('ImportCsvComponent', () => {
  let component: ImportCsvComponent;
  let fixture: ComponentFixture<ImportCsvComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        ImportCsvComponent,
        HttpClientTestingModule,
        BrowserAnimationsModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ImportCsvComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('File Validation', () => {
    it('should accept valid .csv file from drag-drop', () => {
      const file = new File(['test,data\n1,2'], 'test.csv', { type: 'text/csv' });

      component.handleFileSelection(file);

      expect(component.selectedFile()).toBe(file);
      expect(component.errorMessage()).toBeNull();
    });

    it('should reject non-.csv file', () => {
      const file = new File(['test data'], 'test.txt', { type: 'text/plain' });

      component.handleFileSelection(file);

      expect(component.selectedFile()).toBeNull();
      expect(component.errorMessage()).toBe('Only .csv files are allowed');
    });

    it('should reject file larger than 50MB', () => {
      const largeSize = 51 * 1024 * 1024; // 51MB
      const file = new File(['x'.repeat(largeSize)], 'large.csv', { type: 'text/csv' });

      component.handleFileSelection(file);

      expect(component.selectedFile()).toBeNull();
      expect(component.errorMessage()).toBe('File size must not exceed 50MB');
    });

    it('should accept file exactly 50MB', () => {
      const maxSize = 50 * 1024 * 1024; // 50MB
      const file = new File(['x'.repeat(maxSize)], 'max.csv', { type: 'text/csv' });

      component.handleFileSelection(file);

      expect(component.selectedFile()).toBe(file);
      expect(component.errorMessage()).toBeNull();
    });
  });

  describe('File Picker', () => {
    it('should handle file picker selection', () => {
      const file = new File(['test,data'], 'picker.csv', { type: 'text/csv' });
      const event = {
        target: {
          files: [file]
        }
      } as unknown as Event;

      component.onFilePickerChange(event);

      expect(component.selectedFile()).toBe(file);
    });
  });

  describe('CSV Upload and Preview', () => {
    it('should upload CSV and display preview on success', (done) => {
      const file = new File(['Date,Amount\n2025-01-01,10.00'], 'test.csv', { type: 'text/csv' });
      component.selectedFile.set(file);

      const mockResponse = {
        headers: ['Date', 'Amount'],
        sampleRows: [
          { 'Date': '2025-01-01', 'Amount': '10.00' }
        ],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: []
      };

      component.uploadAndPreview();

      const req = httpMock.expectOne('http://localhost:5000/api/import/preview');
      expect(req.request.method).toBe('POST');
      expect(req.request.body.get('file')).toBe(file);

      req.flush(mockResponse);

      setTimeout(() => {
        expect(component.uploading()).toBe(false);
        expect(component.previewData()).toEqual(mockResponse);
        expect(component.errorMessage()).toBeNull();
        done();
      }, 100);
    });

    it('should display error message when upload fails', (done) => {
      const file = new File(['invalid'], 'bad.csv', { type: 'text/csv' });
      component.selectedFile.set(file);

      component.uploadAndPreview();

      const req = httpMock.expectOne('http://localhost:5000/api/import/preview');
      req.flush(
        { message: 'CSV parse error: Invalid format' },
        { status: 400, statusText: 'Bad Request' }
      );

      setTimeout(() => {
        expect(component.uploading()).toBe(false);
        expect(component.errorMessage()).toBe('CSV parse error: Invalid format');
        done();
      }, 100);
    });

    it('should show connection error when server is unreachable', (done) => {
      const file = new File(['test,data'], 'test.csv', { type: 'text/csv' });
      component.selectedFile.set(file);

      component.uploadAndPreview();

      const req = httpMock.expectOne('http://localhost:5000/api/import/preview');
      req.error(new ProgressEvent('error'), { status: 0 });

      setTimeout(() => {
        expect(component.uploading()).toBe(false);
        expect(component.errorMessage()).toContain('Cannot connect to server');
        done();
      }, 100);
    });

    it('should display parse errors in preview', (done) => {
      const file = new File(['Date,Amount\nbad,data'], 'errors.csv', { type: 'text/csv' });
      component.selectedFile.set(file);

      const mockResponse = {
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 0,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [
          { lineNumber: 2, errorMessage: 'Invalid date format', columnName: 'Date' }
        ]
      };

      component.uploadAndPreview();

      const req = httpMock.expectOne('http://localhost:5000/api/import/preview');
      req.flush(mockResponse);

      setTimeout(() => {
        expect(component.previewData()).toEqual(mockResponse);
        expect(component.errorMessage()).toContain('Line 2: Invalid date format');
        done();
      }, 100);
    });
  });

  describe('Preview Display', () => {
    it('should return correct displayed columns', () => {
      component.previewData.set({
        headers: ['Date', 'Description', 'Amount'],
        sampleRows: [],
        totalRowCount: 0,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [],
        suggestions: [],
        columnDetection: null,
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });

      const columns = component.getDisplayedColumns();

      expect(columns).toEqual(['Date', 'Description', 'Amount']);
    });

    it('should return empty array when no preview data', () => {
      component.previewData.set(null);

      const columns = component.getDisplayedColumns();

      expect(columns).toEqual([]);
    });
  });

  describe('Remove File', () => {
    it('should clear selected file and preview data', () => {
      component.selectedFile.set(new File(['test'], 'test.csv', { type: 'text/csv' }));
      component.previewData.set({
        headers: ['Test'],
        sampleRows: [],
        totalRowCount: 0,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [],
        suggestions: [],
        columnDetection: null,
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });
      component.errorMessage.set('Some error');

      component.removeFile();

      expect(component.selectedFile()).toBeNull();
      expect(component.previewData()).toBeNull();
      expect(component.errorMessage()).toBeNull();
    });
  });

  describe('File Size Formatting', () => {
    it('should format bytes correctly', () => {
      expect(component.formatFileSize(0)).toBe('0 Bytes');
      expect(component.formatFileSize(1024)).toBe('1 KB');
      expect(component.formatFileSize(1024 * 1024)).toBe('1 MB');
      expect(component.formatFileSize(1536)).toBe('1.5 KB');
    });
  });

  describe('Drag and Drop', () => {
    it('should set dragOver to true on dragover', () => {
      const event = {
        preventDefault: jest.fn(),
        stopPropagation: jest.fn()
      } as unknown as DragEvent;

      component.onDragOver(event);

      expect(component.dragOver()).toBe(true);
      expect(event.preventDefault).toHaveBeenCalled();
      expect(event.stopPropagation).toHaveBeenCalled();
    });

    it('should set dragOver to false on dragleave', () => {
      component.dragOver.set(true);
      const event = {
        preventDefault: jest.fn(),
        stopPropagation: jest.fn()
      } as unknown as DragEvent;

      component.onDragLeave(event);

      expect(component.dragOver()).toBe(false);
      expect(event.preventDefault).toHaveBeenCalled();
    });

    it('should handle file drop', () => {
      const file = new File(['test'], 'drop.csv', { type: 'text/csv' });
      const event = {
        preventDefault: jest.fn(),
        stopPropagation: jest.fn(),
        dataTransfer: {
          files: [file]
        }
      } as unknown as DragEvent;

      component.onDrop(event);

      expect(component.dragOver()).toBe(false);
      expect(component.selectedFile()).toBe(file);
      expect(event.preventDefault).toHaveBeenCalled();
      expect(event.stopPropagation).toHaveBeenCalled();
    });
  });

  describe('Duplicate Detection', () => {
    it('should detect duplicates in preview response', () => {
      component.previewData.set({
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [
          {
            transactionId: '123',
            date: '2025-01-15',
            payee: 'Whole Foods',
            amount: 89.50,
            category: 'Groceries',
            account: 'Checking'
          }
        ],
        suggestions: [],
        columnDetection: null,
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });

      expect(component.hasDuplicates()).toBe(true);
    });

    it('should not show duplicates when none exist', () => {
      component.previewData.set({
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [],
        suggestions: [],
        columnDetection: null,
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });

      expect(component.hasDuplicates()).toBe(false);
    });

    it('should count skipped duplicates correctly', () => {
      const decisions = new Map<number, 'skip' | 'import'>();
      decisions.set(0, 'skip');
      decisions.set(1, 'import');
      decisions.set(2, 'skip');
      component.duplicateDecisions.set(decisions);

      expect(component.duplicatesSkipped()).toBe(2);
    });

    it('should load category suggestions into map', () => {
      component.previewData.set({
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 2,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [],
        suggestions: [
          {
            transactionIndex: 0,
            suggestedCategoryId: 'cat-1',
            categoryName: 'Groceries',
            confidence: 0.85,
            matchedPattern: 'WHOLE FOODS'
          },
          {
            transactionIndex: 1,
            suggestedCategoryId: 'cat-2',
            categoryName: 'Shopping',
            confidence: 0.75,
            matchedPattern: 'AMAZON'
          }
        ],
        columnDetection: null,
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });

      component.loadCategorySuggestions();

      expect(component.categorySuggestions().size).toBe(2);
      expect(component.getSuggestionForTransaction(0)?.categoryName).toBe('Groceries');
      expect(component.getSuggestionForTransaction(1)?.categoryName).toBe('Shopping');
    });
  });

  describe('Category Suggestions', () => {
    beforeEach(() => {
      component.previewData.set({
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [],
        suggestions: [
          {
            transactionIndex: 0,
            suggestedCategoryId: 'cat-1',
            categoryName: 'Groceries',
            confidence: 0.85,
            matchedPattern: 'WHOLE FOODS'
          }
        ],
        columnDetection: null,
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });
      component.loadCategorySuggestions();
    });

    it('should return confidence class based on confidence level', () => {
      expect(component.getSuggestionConfidenceClass(0.85)).toBe('confidence-high');
      expect(component.getSuggestionConfidenceClass(0.65)).toBe('confidence-medium');
      expect(component.getSuggestionConfidenceClass(0.45)).toBe('confidence-low');
    });

    it('should accept category suggestion', () => {
      const suggestion = component.getSuggestionForTransaction(0);
      expect(suggestion).toBeTruthy();

      component.acceptSuggestion(0, suggestion!);

      expect(component.selectedCategories().get(0)).toBe('cat-1');

      const req = httpMock.expectOne('http://localhost:5000/api/categorize/accept');
      expect(req.request.method).toBe('POST');
      req.flush({ success: true });
    });

    it('should override category suggestion', () => {
      component.overrideSuggestion(0, 'cat-new', 'WHOLE FOODS');

      expect(component.selectedCategories().get(0)).toBe('cat-new');

      const req = httpMock.expectOne('http://localhost:5000/api/categorize/create-rule');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({
        payeePattern: 'WHOLE FOODS',
        categoryId: 'cat-new',
        matchType: 'Contains'
      });
      req.flush({ success: true });
    });

    it('should count transactions with suggestions', () => {
      expect(component.transactionsWithSuggestions()).toBe(1);
    });
  });

  describe('Import Summary', () => {
    it('should calculate import summary correctly', () => {
      component.previewData.set({
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 100,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [],
        suggestions: [],
        columnDetection: null,
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });

      const decisions = new Map<number, 'skip' | 'import'>();
      decisions.set(0, 'skip');
      decisions.set(1, 'skip');
      decisions.set(2, 'skip');
      component.duplicateDecisions.set(decisions);

      component.loadCategorySuggestions();

      const summary = component.importSummary();
      expect(summary).toBeTruthy();
      expect(summary!.total).toBe(100);
      expect(summary!.skipped).toBe(3);
      expect(summary!.readyToImport).toBe(97);
    });

    it('should return null when no preview data', () => {
      component.previewData.set(null);

      expect(component.importSummary()).toBeNull();
    });
  });

  describe('Finalize Import', () => {
    it('should disable finalize when duplicates not reviewed', () => {
      component.previewData.set({
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [
          {
            transactionId: '123',
            date: '2025-01-15',
            payee: 'Test',
            amount: 10.00,
            category: 'Test',
            account: 'Checking'
          }
        ],
        suggestions: [],
        columnDetection: {
          detectedMappings: { Date: 'date', Amount: 'amount' },
          confidenceScores: { date: 0.95, amount: 0.95 },
          warnings: [],
          allRequiredFieldsDetected: true
        },
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });

      expect(component.canFinalizeImport()).toBe(false);
    });

    it('should enable finalize when all duplicates reviewed', () => {
      component.previewData.set({
        headers: ['Date', 'Amount'],
        sampleRows: [],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [
          {
            transactionId: '123',
            date: '2025-01-15',
            payee: 'Test',
            amount: 10.00,
            category: 'Test',
            account: 'Checking'
          }
        ],
        suggestions: [],
        columnDetection: {
          detectedMappings: { Date: 'date', Amount: 'amount' },
          confidenceScores: { date: 0.95, amount: 0.95 },
          warnings: [],
          allRequiredFieldsDetected: true
        },
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: []
      });

      const decisions = new Map<number, 'skip' | 'import'>();
      decisions.set(0, 'skip');
      component.duplicateDecisions.set(decisions);

      expect(component.canFinalizeImport()).toBe(true);
    });
  });

  // Story 2.6: Import Confirmation Tests
  describe('Story 2.6: Import Confirmation Preview', () => {
    beforeEach(() => {
      // Setup mock preview data
      component.previewData.set({
        headers: ['Date', 'Payee', 'Amount'],
        sampleRows: [
          { 'Date': '2025-01-01', 'Payee': 'Whole Foods', 'Amount': '45.23' },
          { 'Date': '2025-01-02', 'Payee': 'Amazon', 'Amount': '89.99' },
          { 'Date': '2025-01-03', 'Payee': 'Starbucks', 'Amount': '5.50' }
        ],
        totalRowCount: 3,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        duplicates: [],
        suggestions: [
          {
            transactionIndex: 0,
            suggestedCategoryId: 'expenses:groceries',
            categoryName: 'Groceries',
            confidence: 0.9,
            matchedPattern: 'Whole Foods'
          }
        ],
        columnDetection: {
          detectedMappings: { Date: 'date', Payee: 'payee', Amount: 'amount' },
          confidenceScores: { date: 0.95, payee: 0.90, amount: 0.95 },
          warnings: [],
          allRequiredFieldsDetected: true
        },
        requiresManualMapping: false,
        savedMapping: null,
        availableHeaders: ['Date', 'Payee', 'Amount']
      });

      // Load category suggestions
      const suggestionMap = new Map();
      suggestionMap.set(0, {
        transactionIndex: 0,
        suggestedCategoryId: 'expenses:groceries',
        categoryName: 'Groceries',
        confidence: 0.9,
        matchedPattern: 'Whole Foods'
      });
      component.categorySuggestions.set(suggestionMap);
    });

    it('should show confirmation preview when finalizeImport is called', () => {
      expect(component.showConfirmationPreview()).toBe(false);

      component.finalizeImport();

      expect(component.showConfirmationPreview()).toBe(true);
    });

    it('should hide confirmation preview when back button is clicked', () => {
      component.showConfirmationPreview.set(true);

      component.backToPreviewFromConfirmation();

      expect(component.showConfirmationPreview()).toBe(false);
    });

    it('should update edited payee when onPayeeEdit is called', () => {
      component.onPayeeEdit(0, 'Modified Payee');

      expect(component.editedPayees().get(0)).toBe('Modified Payee');
    });

    it('should update edited category when onCategoryEdit is called', () => {
      component.onCategoryEdit(1, 'expenses:utilities');

      expect(component.editedCategories().get(1)).toBe('expenses:utilities');
    });

    it('should return edited payee in getCurrentPayee', () => {
      component.editedPayees.set(new Map([[0, 'Edited Payee']]));

      const payee = component.getCurrentPayee(0, 'Original Payee');

      expect(payee).toBe('Edited Payee');
    });

    it('should return original payee if not edited', () => {
      const payee = component.getCurrentPayee(0, 'Original Payee');

      expect(payee).toBe('Original Payee');
    });

    it('should return edited category in getCurrentCategory', () => {
      component.editedCategories.set(new Map([[0, 'expenses:shopping']]));

      const category = component.getCurrentCategory(0);

      expect(category).toBe('expenses:shopping');
    });

    it('should return selected category if no edit exists', () => {
      component.selectedCategories.set(new Map([[0, 'expenses:entertainment']]));

      const category = component.getCurrentCategory(0);

      expect(category).toBe('expenses:entertainment');
    });

    it('should return suggested category if no edit or selection exists', () => {
      const category = component.getCurrentCategory(0);

      expect(category).toBe('expenses:groceries');
    });

    it('should return empty string if no category assigned', () => {
      const category = component.getCurrentCategory(1);

      expect(category).toBe('');
    });

    it('should identify duplicate transactions correctly', () => {
      const decisions = new Map<number, 'skip' | 'import'>();
      decisions.set(0, 'skip');
      component.duplicateDecisions.set(decisions);

      expect(component.isTransactionDuplicate(0)).toBe(true);
      expect(component.isTransactionDuplicate(1)).toBe(false);
    });

    it('should calculate transactions needing categories', () => {
      // Transaction 0 has suggestion, 1 and 2 don't
      const needsCategorization = component.transactionsNeedingCategories();

      expect(needsCategorization).toBe(2);
    });

    it('should validate import is invalid when transactions missing categories', () => {
      expect(component.isImportValid()).toBe(false);
    });

    it('should validate import is valid when all transactions have categories', () => {
      component.editedCategories.set(new Map([
        [1, 'expenses:shopping'],
        [2, 'expenses:entertainment']
      ]));

      expect(component.isImportValid()).toBe(true);
    });

    it('should calculate import summary correctly', () => {
      component.editedCategories.set(new Map([
        [1, 'expenses:shopping'],
        [2, 'expenses:entertainment']
      ]));

      const summary = component.importSummary();

      expect(summary).toBeTruthy();
      expect(summary!.total).toBe(3);
      expect(summary!.skipped).toBe(0);
      expect(summary!.withSuggestions).toBe(1);
      expect(summary!.readyToImport).toBe(3);
      expect(summary!.needsCategorization).toBe(0);
    });

    it('should send confirm import request with correct data', (done) => {
      component.selectedFile.set(new File(['test'], 'test.csv', { type: 'text/csv' }));
      component.editedPayees.set(new Map([[0, 'Edited Whole Foods']]));
      component.editedCategories.set(new Map([
        [1, 'expenses:shopping'],
        [2, 'expenses:entertainment']
      ]));

      component.confirmImport();

      const req = httpMock.expectOne('http://localhost:5000/api/import/confirm');
      expect(req.request.method).toBe('POST');

      const body = req.request.body;
      expect(body.transactions).toBeDefined();
      expect(body.transactions.length).toBe(3);
      expect(body.transactions[0].payee).toBe('Edited Whole Foods');
      expect(body.transactions[0].category).toBe('expenses:groceries');
      expect(body.transactions[1].category).toBe('expenses:shopping');
      expect(body.transactions[2].category).toBe('expenses:entertainment');
      expect(body.fileName).toBe('test.csv');

      const mockResponse = {
        success: true,
        transactionsImported: 3,
        duplicatesSkipped: 0,
        transactionIds: ['id1', 'id2', 'id3']
      };

      req.flush(mockResponse);

      setTimeout(() => {
        expect(component.importing()).toBe(false);
        expect(component.selectedFile()).toBeNull();
        expect(component.previewData()).toBeNull();
        done();
      }, 100);
    });

    it('should filter out duplicate transactions in confirm import', (done) => {
      component.selectedFile.set(new File(['test'], 'test.csv', { type: 'text/csv' }));
      component.editedCategories.set(new Map([
        [0, 'expenses:groceries'],
        [1, 'expenses:shopping'],
        [2, 'expenses:entertainment']
      ]));

      const decisions = new Map<number, 'skip' | 'import'>();
      decisions.set(1, 'skip');
      component.duplicateDecisions.set(decisions);

      component.confirmImport();

      const req = httpMock.expectOne('http://localhost:5000/api/import/confirm');
      const body = req.request.body;

      expect(body.transactions[1].isDuplicate).toBe(true);

      req.flush({
        success: true,
        transactionsImported: 2,
        duplicatesSkipped: 1,
        transactionIds: ['id1', 'id3']
      });

      setTimeout(() => {
        done();
      }, 100);
    });

    it('should handle confirm import API error', (done) => {
      component.selectedFile.set(new File(['test'], 'test.csv', { type: 'text/csv' }));
      component.editedCategories.set(new Map([
        [0, 'expenses:groceries'],
        [1, 'expenses:shopping'],
        [2, 'expenses:entertainment']
      ]));

      component.confirmImport();

      const req = httpMock.expectOne('http://localhost:5000/api/import/confirm');
      req.flush(
        { message: 'Validation error: Missing required fields' },
        { status: 400, statusText: 'Bad Request' }
      );

      setTimeout(() => {
        expect(component.importing()).toBe(false);
        // Component should still have preview data (not reset on error)
        expect(component.previewData()).toBeTruthy();
        done();
      }, 100);
    });

    it('should reset import state after successful import', (done) => {
      component.selectedFile.set(new File(['test'], 'test.csv', { type: 'text/csv' }));
      component.showConfirmationPreview.set(true);
      component.editedPayees.set(new Map([[0, 'Test']]));
      component.editedCategories.set(new Map([
        [0, 'expenses:groceries'],
        [1, 'expenses:shopping'],
        [2, 'expenses:entertainment']
      ]));

      component.confirmImport();

      const req = httpMock.expectOne('http://localhost:5000/api/import/confirm');
      req.flush({
        success: true,
        transactionsImported: 3,
        duplicatesSkipped: 0,
        transactionIds: ['id1', 'id2', 'id3']
      });

      setTimeout(() => {
        expect(component.selectedFile()).toBeNull();
        expect(component.previewData()).toBeNull();
        expect(component.showConfirmationPreview()).toBe(false);
        expect(component.editedPayees().size).toBe(0);
        expect(component.editedCategories().size).toBe(0);
        done();
      }, 100);
    });

    // P1 Test: Explicit test for success snackbar with dashboard navigation (QA Gap)
    it('should display success snackbar with dashboard navigation button on successful import', (done) => {
      component.selectedFile.set(new File(['test'], 'test.csv', { type: 'text/csv' }));
      component.previewData.set({
        headers: ['Date', 'Payee', 'Amount'],
        sampleRows: [{ Date: '2025-01-15', Payee: 'Test Store', Amount: '50.00' }],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        requiresManualMapping: false,
        availableHeaders: ['Date', 'Payee', 'Amount'],
        duplicates: [],
        suggestions: []
      });
      component.showConfirmationPreview.set(true);

      // Spy on snackbar to verify it's called with correct parameters
      const snackBarSpy = jest.spyOn(component['snackBar'], 'open');
      const routerSpy = jest.spyOn(component['router'], 'navigate');

      component.confirmImport();

      const req = httpMock.expectOne('http://localhost:5000/api/import/confirm');
      req.flush({
        success: true,
        transactionsImported: 1,
        duplicatesSkipped: 0,
        transactionIds: ['guid1']
      });

      setTimeout(() => {
        // Verify snackbar was called with success message and "View Dashboard" action
        expect(snackBarSpy).toHaveBeenCalledWith(
          'Imported 1 transactions successfully',
          'View Dashboard',
          expect.objectContaining({
            duration: 10000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            panelClass: ['success-snackbar']
          })
        );

        // Verify snackbar returns a ref with onAction method
        const snackBarRef = snackBarSpy.mock.results[0].value;
        expect(snackBarRef).toBeDefined();
        expect(snackBarRef.onAction).toBeDefined();

        // Trigger the action button to verify navigation
        if (snackBarRef && snackBarRef.onAction) {
          snackBarRef.onAction().subscribe(() => {
            expect(routerSpy).toHaveBeenCalledWith(['/dashboard']);
            done();
          });
        } else {
          done();
        }
      }, 100);
    });

    // P1 Test: Explicit test for error snackbar with retry button (QA Gap)
    it('should display error snackbar with retry button on import failure', (done) => {
      component.selectedFile.set(new File(['test'], 'test.csv', { type: 'text/csv' }));
      component.previewData.set({
        headers: ['Date', 'Payee', 'Amount'],
        sampleRows: [{ Date: '2025-01-15', Payee: 'Test Store', Amount: '50.00' }],
        totalRowCount: 1,
        detectedDelimiter: 'Comma',
        detectedEncoding: 'UTF-8',
        errors: [],
        requiresManualMapping: false,
        availableHeaders: ['Date', 'Payee', 'Amount'],
        duplicates: [],
        suggestions: []
      });
      component.showConfirmationPreview.set(true);

      // Spy on snackbar to verify error handling
      const snackBarSpy = jest.spyOn(component['snackBar'], 'open');

      component.confirmImport();

      const req = httpMock.expectOne('http://localhost:5000/api/import/confirm');
      req.flush(
        { message: 'Import validation failed: 3 transactions missing categories' },
        { status: 400, statusText: 'Bad Request' }
      );

      setTimeout(() => {
        // Verify error snackbar was called with error message and "Retry" action
        expect(snackBarSpy).toHaveBeenCalledWith(
          'Import validation failed: 3 transactions missing categories',
          'Retry',
          expect.objectContaining({
            duration: 10000,
            horizontalPosition: 'center',
            verticalPosition: 'bottom',
            panelClass: ['error-snackbar']
          })
        );

        // Verify component state is not reset (preview remains for retry)
        expect(component.previewData()).not.toBeNull();
        expect(component.showConfirmationPreview()).toBe(true);
        done();
      }, 100);
    });
  });
});
