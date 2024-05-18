import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { profileIsCompletedGuard } from './profile-is-completed.guard';

describe('profileIsCompletedGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => profileIsCompletedGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
