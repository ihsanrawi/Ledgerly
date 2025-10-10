import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ImportCsvComponent } from './import-csv.component';

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
        errors: []
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
        errors: []
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
});
