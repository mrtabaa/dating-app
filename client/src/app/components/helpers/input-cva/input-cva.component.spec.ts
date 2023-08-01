import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InputCvaComponent } from './input-cva.component';

describe('InputCvaComponent', () => {
  let component: InputCvaComponent;
  let fixture: ComponentFixture<InputCvaComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [InputCvaComponent]
    });
    fixture = TestBed.createComponent(InputCvaComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
