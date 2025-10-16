import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ManualMappingComponent } from './manual-mapping.component';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { signal } from '@angular/core';

describe('ManualMappingComponent', () => {
  let component: ManualMappingComponent;
  let fixture: ComponentFixture<ManualMappingComponent>;
  let httpMock: HttpTestingController;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  const mockHeaders = ['Date', 'Amount', 'Description', 'Memo', 'Balance'];
  const mockSampleRows = [
    { Date: '2024-01-01', Amount: '100.00', Description: 'Test 1', Memo: 'Note 1', Balance: '500.00' },
    { Date: '2024-01-02', Amount: '50.00', Description: 'Test 2', Memo: 'Note 2', Balance: '550.00' },
    { Date: '2024-01-03', Amount: '-25.00', Description: 'Test 3', Memo: 'Note 3', Balance: '525.00' }
  ];

  beforeEach(async () => {
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      imports: [
        ManualMappingComponent,
        HttpClientTestingModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: MatSnackBar, useValue: snackBarSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ManualMappingComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);

    // Set required inputs using signals
    fixture.componentRef.setInput('availableHeaders', mockHeaders);
    fixture.componentRef.setInput('sampleRows', mockSampleRows);
    fixture.componentRef.setInput('savedMapping', null);

    fixture.detectChanges();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Initialization', () => {
    it('should initialize with all headers unmapped', () => {
      expect(component.unmappedHeaders()).toEqual(mockHeaders);
      expect(component.mappings()).toEqual({});
    });

    it('should pre-fill mappings from saved mapping', () => {
      const savedMapping = {
        'Date': 'date',
        'Amount': 'amount',
        'Description': 'description'
      };

      fixture.componentRef.setInput('savedMapping', savedMapping);
      component.ngOnInit();
      fixture.detectChanges();

      const mappings = component.mappings();
      expect(mappings.date).toBe('Date');
      expect(mappings.amount).toBe('Amount');
      expect(mappings.description).toBe('Description');
      expect(component.unmappedHeaders()).toEqual(['Memo', 'Balance']);
    });
  });

  describe('Drag and Drop', () => {
    it('should map header to field type on drop', () => {
      const mockEvent = {
        item: { data: 'Date' }
      } as any;

      component.onDrop(mockEvent, 'date');

      expect(component.mappings().date).toBe('Date');
      expect(component.unmappedHeaders()).not.toContain('Date');
    });

    it('should remove header from previous mapping when dropped to new field', () => {
      const mockEvent1 = { item: { data: 'Date' } } as any;
      const mockEvent2 = { item: { data: 'Date' } } as any;

      component.onDrop(mockEvent1, 'date');
      expect(component.mappings().date).toBe('Date');

      component.onDrop(mockEvent2, 'description');
      expect(component.mappings().date).toBeUndefined();
      expect(component.mappings().description).toBe('Date');
    });

    it('should remove mapping and return header to unmapped list', () => {
      const mockEvent = { item: { data: 'Amount' } } as any;
      component.onDrop(mockEvent, 'amount');

      component.removeMapping('amount');

      expect(component.mappings().amount).toBeUndefined();
      expect(component.unmappedHeaders()).toContain('Amount');
    });
  });

  describe('Validation', () => {
    it('should have requiredFieldsMapped false when required fields not mapped', () => {
      expect(component.requiredFieldsMapped()).toBe(false);
    });

    it('should have requiredFieldsMapped true when all required fields mapped', () => {
      component.onDrop({ item: { data: 'Date' } } as any, 'date');
      component.onDrop({ item: { data: 'Amount' } } as any, 'amount');
      component.onDrop({ item: { data: 'Description' } } as any, 'description');

      expect(component.requiredFieldsMapped()).toBe(true);
    });

    it('should show validation errors for missing required fields', () => {
      const errors = component.validationErrors();

      expect(errors).toContain('Date column is required');
      expect(errors).toContain('Amount column is required');
      expect(errors).toContain('Description column is required');
    });

    it('should clear validation errors when required fields are mapped', () => {
      component.onDrop({ item: { data: 'Date' } } as any, 'date');
      component.onDrop({ item: { data: 'Amount' } } as any, 'amount');
      component.onDrop({ item: { data: 'Description' } } as any, 'description');

      const errors = component.validationErrors();
      expect(errors.length).toBe(0);
    });
  });

  describe('Save Mapping', () => {
    beforeEach(() => {
      component.onDrop({ item: { data: 'Date' } } as any, 'date');
      component.onDrop({ item: { data: 'Amount' } } as any, 'amount');
      component.onDrop({ item: { data: 'Description' } } as any, 'description');
      component.bankIdentifier.set('Chase Checking');
    });

    it('should not save mapping if required fields not mapped', () => {
      component.mappings.set({});
      component.saveMapping();

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Please map all required fields before saving',
        'Close',
        jasmine.any(Object)
      );
    });

    it('should not save mapping if bank identifier is empty', () => {
      component.bankIdentifier.set('');
      component.saveMapping();

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Please enter a bank identifier',
        'Close',
        jasmine.any(Object)
      );
    });

    it('should save mapping successfully', () => {
      component.saveMapping();

      const req = httpMock.expectOne('http://localhost:5000/api/import/save-mapping');
      expect(req.request.method).toBe('POST');
      expect(req.request.body.bankIdentifier).toBe('Chase Checking');
      expect(req.request.body.columnMappings).toEqual({
        'Date': 'date',
        'Amount': 'amount',
        'Description': 'description'
      });
      expect(req.request.body.headerSignature).toEqual(mockHeaders);

      req.flush({ id: 'test-id', message: 'Success' });

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Mapping saved for Chase Checking',
        'Close',
        jasmine.any(Object)
      );
    });

    it('should handle save mapping error', () => {
      component.saveMapping();

      const req = httpMock.expectOne('http://localhost:5000/api/import/save-mapping');
      req.flush({ message: 'Validation failed' }, { status: 400, statusText: 'Bad Request' });

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Validation failed',
        'Close',
        jasmine.any(Object)
      );
    });
  });

  describe('Proceed to Next', () => {
    it('should emit mappingComplete with correct format', () => {
      spyOn(component.mappingComplete, 'emit');

      component.onDrop({ item: { data: 'Date' } } as any, 'date');
      component.onDrop({ item: { data: 'Amount' } } as any, 'amount');
      component.onDrop({ item: { data: 'Description' } } as any, 'description');
      component.onDrop({ item: { data: 'Memo' } } as any, 'memo');

      component.proceedToNext();

      expect(component.mappingComplete.emit).toHaveBeenCalledWith({
        'Date': 'date',
        'Amount': 'amount',
        'Description': 'description',
        'Memo': 'memo'
      });
    });

    it('should not proceed if required fields not mapped', () => {
      spyOn(component.mappingComplete, 'emit');

      component.proceedToNext();

      expect(component.mappingComplete.emit).not.toHaveBeenCalled();
    });
  });

  describe('Preview Table Helpers', () => {
    it('should get field type for mapped header', () => {
      component.onDrop({ item: { data: 'Date' } } as any, 'date');

      expect(component.getFieldTypeForHeader('Date')).toBe('date');
    });

    it('should return null for unmapped header', () => {
      expect(component.getFieldTypeForHeader('Date')).toBeNull();
    });

    it('should identify required fields correctly', () => {
      expect(component.isRequiredField('date')).toBe(true);
      expect(component.isRequiredField('amount')).toBe(true);
      expect(component.isRequiredField('description')).toBe(true);
      expect(component.isRequiredField('memo')).toBe(false);
      expect(component.isRequiredField('balance')).toBe(false);
    });

    it('should get correct chip color for field types', () => {
      expect(component.getChipColor('date')).toBe('primary');
      expect(component.getChipColor('amount')).toBe('primary');
      expect(component.getChipColor('description')).toBe('primary');
      expect(component.getChipColor('memo')).toBe('accent');
      expect(component.getChipColor('balance')).toBe('accent');
    });
  });
});
