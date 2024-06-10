import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { ResponsiveService } from '../services/responsive.service';

export const mobileGuard: CanActivateFn = () => {
  const isMobileSig = inject(ResponsiveService).isMobileSig;
  const router = inject(Router);

  if (isMobileSig())
    router.navigate(['/main']);

  return true;
};
