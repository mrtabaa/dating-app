import { CanDeactivateFn } from '@angular/router';
import { UserEditComponent } from '../components/user/user-edit/user-edit.component';

export const preventUnsavedChangesGuard: CanDeactivateFn<UserEditComponent> = (component, currentRoute, currentState, nextState) => {
  if (component.userEditFg?.dirty) {
    return confirm('Are you sure you want to continue? Any unsaved changes will be lost!');
  }
  return true;
};
