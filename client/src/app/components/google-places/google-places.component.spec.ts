import { ComponentFixture, TestBed } from '@angular/core/testing';

import { GooglePlacesComponent } from './google-places.component';

describe('GooglePlacesComponent', () => {
  let component: GooglePlacesComponent;
  let fixture: ComponentFixture<GooglePlacesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GooglePlacesComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(GooglePlacesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
