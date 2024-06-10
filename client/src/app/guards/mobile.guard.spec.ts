import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

import { mobileGuard } from './mobile.guard';

describe('mobileGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => mobileGuard(...guardParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
