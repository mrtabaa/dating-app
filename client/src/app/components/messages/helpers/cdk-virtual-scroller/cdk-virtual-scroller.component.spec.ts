import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CdkVirtualScrollerComponent } from './cdk-virtual-scroller.component';

describe('CdkVirtualScrollerComponent', () => {
  let component: CdkVirtualScrollerComponent;
  let fixture: ComponentFixture<CdkVirtualScrollerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CdkVirtualScrollerComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(CdkVirtualScrollerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
