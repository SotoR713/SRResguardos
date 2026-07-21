import { Routes } from '@angular/router';

import { Activos } from './components/activos/activos';
import { Empleados } from './components/empleados/empleados';
import { Empresas } from './components/empresas/empresas';
import { EntregarActivo } from './components/entregar-activo/entregar-activo';
import { RecogerActivo } from './components/recoger-activo/recoger-activo';
import { TiposActivo } from './components/tipos-activo/tipos-activo';

export const routes: Routes = [
  { path: 'activos', component: Activos },
  { path: 'empleados', component: Empleados },
  { path: 'empresas', component: Empresas },
  { path: 'entregar-activo', component: EntregarActivo },
  { path: 'recoger-activo', component: RecogerActivo },
  { path: 'tipos-activo', component: TiposActivo },

  // Redirección al abrir la aplicación
  { path: '', redirectTo: 'activos', pathMatch: 'full' },
];