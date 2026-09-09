import { Routes } from '@angular/router';
import { AdminDashboard } from './features/admin-dashboard/admin-dashboard';
import { Dashboard } from './features/dashboard/dashboard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'sponsor' },
  { path: 'sponsor', component: Dashboard, data: { leaId: 'dcps', locked: true } },
  { path: 'admin', component: AdminDashboard },
  { path: 'admin/lea/:leaId', component: Dashboard, data: { locked: false } },
  { path: '**', redirectTo: 'sponsor' },
];
