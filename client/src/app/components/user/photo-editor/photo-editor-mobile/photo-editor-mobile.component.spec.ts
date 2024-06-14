import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PhotoEditorMobileComponent } from './photo-editor-mobile.component';

describe('PhotoEditorMobileComponent', () => {
  let component: PhotoEditorMobileComponent;
  let fixture: ComponentFixture<PhotoEditorMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PhotoEditorMobileComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(PhotoEditorMobileComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
