import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ImportRulesComponent } from './import-rules.component';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('ImportRulesComponent', () => {
  let component: ImportRulesComponent;
  let fixture: ComponentFixture<ImportRulesComponent>;
  let httpMock: HttpTestingController;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;

  const mockMappings = [
    {
      id: '1',
      bankIdentifier: 'Chase Checking',
      columnMappings: {
        'Date': 'date',
        'Amount': 'amount',
        'Description': 'description',
        'Memo': 'memo'
      },
      createdAt: '2024-01-01T10:00:00Z',
      lastUsedAt: '2024-01-15T14:30:00Z',
      timesUsed: 5
    },
    {
      id: '2',
      bankIdentifier: 'Bank of America',
      columnMappings: {
        'Transaction Date': 'date',
        'Debit': 'amount',
        'Payee': 'description'
      },
      createdAt: '2024-01-05T12:00:00Z',
      lastUsedAt: '2024-01-20T09:15:00Z',
      timesUsed: 3
    }
  ];

  beforeEach(async () => {
    snackBarSpy = jasmine.createSpyObj('MatSnackBar', ['open']);
    dialogSpy = jasmine.createSpyObj('MatDialog', ['open']);

    await TestBed.configureTestingModule({
      imports: [
        ImportRulesComponent,
        HttpClientTestingModule,
        BrowserAnimationsModule
      ],
      providers: [
        { provide: MatSnackBar, useValue: snackBarSpy },
        { provide: MatDialog, useValue: dialogSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ImportRulesComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('Component Initialization', () => {
    it('should load saved mappings on init', () => {
      fixture.detectChanges(); // triggers ngOnInit

      const req = httpMock.expectOne('http://localhost:5000/api/import/mappings');
      expect(req.request.method).toBe('GET');

      req.flush(mockMappings);

      expect(component.savedMappings()).toEqual(mockMappings);
      expect(component.loading()).toBe(false);
    });

    it('should handle error when loading mappings', () => {
      fixture.detectChanges();

      const req = httpMock.expectOne('http://localhost:5000/api/import/mappings');
      req.flush({ message: 'Server error' }, { status: 500, statusText: 'Internal Server Error' });

      expect(component.loading()).toBe(false);
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Server error',
        'Close',
        jasmine.any(Object)
      );
    });
  });

  describe('Display Helpers', () => {
    it('should format mapped columns display correctly', () => {
      const result = component.getMappedColumnsDisplay(mockMappings[0].columnMappings);
      expect(result).toBe('4 columns (3 required, 1 optional)');
    });

    it('should format mapped columns display with only required fields', () => {
      const mappings = {
        'Date': 'date',
        'Amount': 'amount',
        'Description': 'description'
      };
      const result = component.getMappedColumnsDisplay(mappings);
      expect(result).toBe('3 columns (3 required)');
    });

    it('should format date correctly', () => {
      const result = component.formatDate('2024-01-15T14:30:00Z');
      expect(result).toContain('Jan');
      expect(result).toContain('15');
      expect(result).toContain('2024');
    });
  });

  describe('View Mapping', () => {
    beforeEach(() => {
      component.savedMappings.set(mockMappings);
    });

    it('should display mapping details in snackbar', () => {
      component.viewMapping(mockMappings[0]);

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        jasmine.stringContaining('Chase Checking'),
        'Close',
        jasmine.any(Object)
      );
      expect(snackBarSpy.open).toHaveBeenCalledWith(
        jasmine.stringContaining('Date → date'),
        'Close',
        jasmine.any(Object)
      );
    });
  });

  describe('Delete Mapping', () => {
    beforeEach(() => {
      component.savedMappings.set(mockMappings);
      spyOn(window, 'confirm');
    });

    it('should not delete if user cancels confirmation', () => {
      (window.confirm as jasmine.Spy).and.returnValue(false);

      component.deleteMapping(mockMappings[0]);

      httpMock.expectNone('http://localhost:5000/api/import/mappings/1');
    });

    it('should delete mapping successfully', () => {
      (window.confirm as jasmine.Spy).and.returnValue(true);

      component.deleteMapping(mockMappings[0]);

      const req = httpMock.expectOne('http://localhost:5000/api/import/mappings/1');
      expect(req.request.method).toBe('DELETE');

      req.flush({});

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Mapping for Chase Checking deleted',
        'Close',
        jasmine.any(Object)
      );

      // Should reload mappings after delete
      const reloadReq = httpMock.expectOne('http://localhost:5000/api/import/mappings');
      reloadReq.flush([mockMappings[1]]);
    });

    it('should handle delete error', () => {
      (window.confirm as jasmine.Spy).and.returnValue(true);

      component.deleteMapping(mockMappings[0]);

      const req = httpMock.expectOne('http://localhost:5000/api/import/mappings/1');
      req.flush({ message: 'Delete failed' }, { status: 400, statusText: 'Bad Request' });

      expect(snackBarSpy.open).toHaveBeenCalledWith(
        'Delete failed',
        'Close',
        jasmine.any(Object)
      );
    });
  });

  describe('Refresh', () => {
    it('should reload mappings when refresh is called', () => {
      component.savedMappings.set([]);

      component.refresh();

      const req = httpMock.expectOne('http://localhost:5000/api/import/mappings');
      req.flush(mockMappings);

      expect(component.savedMappings()).toEqual(mockMappings);
    });
  });

  describe('Table Display', () => {
    it('should display correct column definitions', () => {
      expect(component.displayedColumns).toEqual([
        'bankIdentifier',
        'mappedColumns',
        'timesUsed',
        'lastUsedAt',
        'actions'
      ]);
    });
  });
});
