import { Routes } from '@angular/router';
import { LayoutComponent } from './core/layout/layout';
import { authGuard } from './core/guards/auth';

export const routes: Routes = [
    {
        path: 'auth',
        children: [
            {
                path: 'login',
                loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
            },
            {
                path: 'register',
                loadComponent: () => import('./features/auth/register/register').then(m => m.RegisterComponent)
            }
        ]
    },
    {
        path: 'app',
        component: LayoutComponent,
        canActivate: [authGuard],
        children: [
            {
                path: 'dashboard',
                loadComponent: () => import('./features/dashboard/dashboard').then(m => m.DashboardComponent)
            },
            {
                path: 'categorias',
                loadComponent: () => import('./features/categorias/categorias').then(m => m.CategoriasComponent)
            },
            {
                path: 'lancamentos',
                loadComponent: () => import('./features/lancamentos/lancamentos').then(m => m.LancamentosComponent)
            },
            {
                path: '',
                redirectTo: 'dashboard',
                pathMatch: 'full'
            }
        ]
    },
    {
        path: '',
        redirectTo: 'auth/login',
        pathMatch: 'full'
    },
    {
        path: '**',
        redirectTo: 'auth/login'
    }
];
