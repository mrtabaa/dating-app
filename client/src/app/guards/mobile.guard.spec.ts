import { TestBed } from '@angular/core/testing';
import { CanActivateFn } from '@angular/router';

<<<<<<<< HEAD:client/src/app/guards/complete-profile.guard.spec.ts
import { completeProfileGuard } from './complete-profile.guard';

describe('profileCompleteGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) =>
    TestBed.runInInjectionContext(() => completeProfileGuard(...guardParameters));
========
import { mobileGuard } from './mobile.guard';

describe('mobileGuard', () => {
  const executeGuard: CanActivateFn = (...guardParameters) => 
      TestBed.runInInjectionContext(() => mobileGuard(...guardParameters));
>>>>>>>> incomplete:client/src/app/guards/mobile.guard.spec.ts

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeGuard).toBeTruthy();
  });
});
