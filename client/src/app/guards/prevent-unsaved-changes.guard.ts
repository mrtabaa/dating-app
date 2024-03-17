import { CanDeactivateFn } from '@angular/router';
import { UserEditComponent } from '../components/user/user-edit/user-edit.component';
import { inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { map } from 'rxjs';
import { ConfirmComponent } from '../components/_modals/confirm/confirm.component';

export const preventUnsavedChangesGuard: CanDeactivateFn<UserEditComponent> = (component, currentRoute, currentState, nextState) => {
  const dialog = inject(MatDialog);

  if (component.userEditFg?.dirty) {
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
