import { Injectable } from '@angular/core';
import { UserEditComponent } from '../components/user/user-edit/user-edit.component';

@Injectable({
  providedIn: 'root'
})
export class PreventUnsavedChangesGuard {
  canDeactivate(
    component: UserEditComponent,): boolean {
    if (component.userEditFg.dirty) {
      return confirm('Are you sure you want to continue? Any unsaved changes will be lost!');
    }
    return true;
  }
}
