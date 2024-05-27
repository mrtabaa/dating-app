import { TestBed } from '@angular/core/testing';

import { GooglePlaceService } from './google-place.service';

describe('GooglePlaceService', () => {
  let service: GooglePlaceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GooglePlaceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
