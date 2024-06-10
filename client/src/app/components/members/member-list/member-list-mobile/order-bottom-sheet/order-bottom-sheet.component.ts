import { Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-order-bottom-sheet',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule, MatSelectModule
  ],
  templateUrl: './order-bottom-sheet.component.html',
  styleUrl: './order-bottom-sheet.component.scss'
})
export class OrderBottomSheetComponent {

}
