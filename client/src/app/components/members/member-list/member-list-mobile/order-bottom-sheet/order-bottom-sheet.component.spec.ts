import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderBottomSheetComponent } from './order-bottom-sheet.component';

describe('OrderBottomSheetComponent', () => {
  let component: OrderBottomSheetComponent;
  let fixture: ComponentFixture<OrderBottomSheetComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OrderBottomSheetComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(OrderBottomSheetComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
