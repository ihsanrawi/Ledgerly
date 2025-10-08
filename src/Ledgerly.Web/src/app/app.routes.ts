import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/balance',
    pathMatch: 'full'
  },
  {
    path: 'balance',
    loadComponent: () => import('./features/balance/balance-display.component').then(m => m.BalanceDisplayComponent)
  }
];
