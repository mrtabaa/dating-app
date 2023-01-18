import { TestBed } from '@angular/core/testing';

import { AuthLoggedInGuard } from './auth-logged-in.guard';

describe('AuthLoggedInGuard', () => {
  let guard: AuthLoggedInGuard;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    guard = TestBed.inject(AuthLoggedInGuard);
  });

  it('should be created', () => {
    expect(guard).toBeTruthy();
  });
});
