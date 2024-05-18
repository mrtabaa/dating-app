import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { profileCompleteGuard } from './profile-complete.guard';

describe('profileCompleteGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => profileCompleteGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
