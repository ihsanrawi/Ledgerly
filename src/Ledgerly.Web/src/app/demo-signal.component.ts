import { Component, signal, computed } from '@angular/core';

/**
 * Demo component to verify Jest + Angular Signals integration
 * This component demonstrates that Signals work correctly with Jest test infrastructure
 */
@Component({
  selector: 'app-demo-signal',
  standalone: true,
  template: `<div>{{displayValue()}}</div>`
})
export class DemoSignalComponent {
  // Writable signal
  count = signal(0);

  // Computed signal
  displayValue = computed(() => `Count: ${this.count()}`);

  increment() {
    this.count.update(c => c + 1);
  }

  reset() {
    this.count.set(0);
  }
}
