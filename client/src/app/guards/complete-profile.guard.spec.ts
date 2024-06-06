import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { completeProfileGuard } from './complete-profile.guard';

describe('profileCompleteGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => completeProfileGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
