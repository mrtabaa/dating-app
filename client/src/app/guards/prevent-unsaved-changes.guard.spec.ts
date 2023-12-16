import { TestBed } from '@angular/core/testing';
import { CanDeactivateFn } from '@angular/router';

import { preventUnsavedChangesGuard } from './prevent-unsaved-changes.guard';
import { UserEditComponent } from '../components/user/user-edit/user-edit.component';

describe('preventUnsavedChangesGuard', () => {
  const executeGuard: CanDeactivateFn<UserEditComponent> = (...guardParameters) => 
      TestBed.runInInjectionContext(() => preventUnsavedChangesGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
