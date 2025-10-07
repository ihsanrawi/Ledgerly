import { TestBed } from '@angular/core/testing';
import { DemoSignalComponent } from './demo-signal.component';

/**
 * Tests demonstrating Jest + Angular Signals integration
 * Verifies AC3: "Jest configured for Angular unit tests (Signals support)"
 */
describe('DemoSignalComponent - Signals Integration', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DemoSignalComponent],
    }).compileComponents();
  });

  it('should create component with signals', () => {
    const fixture = TestBed.createComponent(DemoSignalComponent);
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
    expect(component.count()).toBe(0);
  });

  it('should update writable signal with set()', () => {
    const fixture = TestBed.createComponent(DemoSignalComponent);
    const component = fixture.componentInstance;

    component.count.set(5);

    expect(component.count()).toBe(5);
  });

  it('should update writable signal with update()', () => {
    const fixture = TestBed.createComponent(DemoSignalComponent);
    const component = fixture.componentInstance;

    component.increment();
    component.increment();

    expect(component.count()).toBe(2);
  });

  it('should compute derived value with computed signal', () => {
    const fixture = TestBed.createComponent(DemoSignalComponent);
    const component = fixture.componentInstance;

    expect(component.displayValue()).toBe('Count: 0');

    component.count.set(10);

    expect(component.displayValue()).toBe('Count: 10');
  });

  it('should reset signal value', () => {
    const fixture = TestBed.createComponent(DemoSignalComponent);
    const component = fixture.componentInstance;

    component.count.set(100);
    expect(component.count()).toBe(100);

    component.reset();
    expect(component.count()).toBe(0);
  });
});
