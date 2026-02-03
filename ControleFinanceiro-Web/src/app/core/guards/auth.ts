import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const authGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    // Directly check localStorage as a fallback to avoid race conditions with Signal initialization
    const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;
    const isAuth = authService.isAuthenticated() || (token && token !== 'undefined' && token !== 'null');

    if (isAuth) {
        if (!authService.isAuthenticated()) {
            authService.checkAuth(); // Re-sync state if needed
        }
        return true;
    }

    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
};
