import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastrService } from 'ngx-toastr';

export const adminGuard: CanActivateFn = (route, state) => {
    const authService = inject(AuthService);
    const router = inject(Router);
    const toastr = inject(ToastrService);

    const user = authService.currentUserValue;

    if (user && user.role === 'Admin') {
        return true;
    }

    toastr.error('Access denied. Admin privileges required.');
    router.navigate(['/dashboard']);
    return false;
};
