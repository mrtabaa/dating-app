import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MemberService } from '../../../../../services/member.service';
import { MemberParams } from '../../../../../models/helpers/member-params';
import { tap } from 'rxjs';

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
  private _memberService = inject(MemberService);
  private _fb = inject(FormBuilder);

  memberParams: MemberParams | undefined;

  orderOptions: string[] = ['lastActive', 'created', 'age'];
  orderOptionsView: string[] = ['Last Active', 'Created', 'Age'];

  isSuccess = false;

  orderByCtrl = this._fb.control('lastActive', [])

  getOrderValue(): void {
    console.log('OrderSheet:', this.orderByCtrl.value);
  }

  getMembers(): void {
    this.setMemberParams()

    this._memberService.getMembers().pipe(
      tap(res => {
        if (res)
          this.isSuccess = true;
      })
    );
  }

  setMemberParams(): MemberParams | undefined {
    this.isSuccess = false;

    this.memberParams = this._memberService.memberParams;

    if (this.memberParams && this.orderByCtrl.value) {
      this.memberParams.orderBy = this.orderByCtrl.value;
      this._memberService.setMemberParams(this.memberParams);
    }

    return this.memberParams;
  }
}


