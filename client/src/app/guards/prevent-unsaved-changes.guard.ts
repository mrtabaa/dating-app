import { CanDeactivateFn } from '@angular/router';
import { UserEditComponent } from '../components/user/user-edit/user-edit.component';
import { inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { map } from 'rxjs';
import { ConfirmComponent } from '../components/_modals/confirm/confirm.component';
import { CommonService } from '../services/common.service';

// TODO Use it for User Management
export const preventUnsavedChangesGuard: CanDeactivateFn<UserEditComponent> = () => {
  const dialog = inject(MatDialog);
  const commonService = inject(CommonService);

  if (commonService.isPreventingLeavingPage) {
    const dialogRef = dialog.open(ConfirmComponent);

    return dialogRef.afterClosed()
      .pipe(
        map((action: boolean) => {
          if (action) return true

          return false
        }
        )
      )
  }
  return true;
};
