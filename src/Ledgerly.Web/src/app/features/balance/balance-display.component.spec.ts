import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { BalanceDisplayComponent } from './balance-display.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

describe('BalanceDisplayComponent', () => {
  let component: BalanceDisplayComponent;
  let fixture: ComponentFixture<BalanceDisplayComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        BalanceDisplayComponent,
        HttpClientTestingModule,
        BrowserAnimationsModule
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BalanceDisplayComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should fetch balances on init', () => {
    const mockResponse = {
      balances: [
        {
          account: 'Assets:Checking',
          balance: 1000,
          depth: 2,
          children: []
        }
      ],
      asOfDate: '2025-01-15T10:30:00Z'
    };

    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5000/api/balance');
    expect(req.request.method).toBe('GET');
    req.flush(mockResponse);

    expect(component.balances()).toEqual(mockResponse.balances);
    expect(component.loading()).toBe(false);
    expect(component.error()).toBeNull();
  });

  it('should set error on HTTP failure', () => {
    const errorMessage = 'Network error';

    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5000/api/balance');
    req.error(new ProgressEvent('error'), { status: 500, statusText: errorMessage });

    expect(component.loading()).toBe(false);
    expect(component.error()).toContain('Failed to load balances');
  });

  it('should update tree data source when balances change', () => {
    const mockResponse = {
      balances: [
        {
          account: 'Assets',
          balance: 0,
          depth: 1,
          children: [
            {
              account: 'Assets:Checking',
              balance: 1000,
              depth: 2,
              children: []
            }
          ]
        }
      ],
      asOfDate: '2025-01-15T10:30:00Z'
    };

    fixture.detectChanges();

    const req = httpMock.expectOne('http://localhost:5000/api/balance');
    req.flush(mockResponse);

    expect(component.dataSource.data).toEqual(mockResponse.balances);
  });

  it('should have hasChild return true for nodes with children', () => {
    const nodeWithChildren = {
      account: 'Assets',
      balance: 0,
      depth: 1,
      children: [{ account: 'Assets:Checking', balance: 1000, depth: 2, children: [] }]
    };

    const nodeWithoutChildren = {
      account: 'Assets:Checking',
      balance: 1000,
      depth: 2,
      children: []
    };

    expect(component.hasChild(0, nodeWithChildren)).toBe(true);
    expect(component.hasChild(0, nodeWithoutChildren)).toBe(false);
  });

  it('should refresh balances when refresh is called', () => {
    const mockResponse = {
      balances: [
        {
          account: 'Assets:Checking',
          balance: 2000,
          depth: 2,
          children: []
        }
      ],
      asOfDate: '2025-01-15T11:00:00Z'
    };

    fixture.detectChanges();

    // Initial request
    const req1 = httpMock.expectOne('http://localhost:5000/api/balance');
    req1.flush({ balances: [], asOfDate: '2025-01-15T10:00:00Z' });

    // Refresh
    component.refresh();

    const req2 = httpMock.expectOne('http://localhost:5000/api/balance');
    req2.flush(mockResponse);

    expect(component.balances()).toEqual(mockResponse.balances);
  });
});
