import {Component, inject} from '@angular/core';
import {AbstractControl, FormBuilder, FormControl, FormsModule, ReactiveFormsModule} from '@angular/forms';
import {MatButtonModule} from '@angular/material/button';
import {MatDividerModule} from '@angular/material/divider';
import {MatSelectModule} from '@angular/material/select';
import {MatSliderModule} from '@angular/material/slider';
import {MemberService} from '../../../../../services/member.service';
import {InputCvaComponent} from "../../../../_helpers/input-cva/input-cva.component";
import {MemberParams} from "../../../../../models/helpers/member-params";

@Component({
  selector: 'app-filter-bottom-sheet',
  imports: [
    FormsModule, ReactiveFormsModule, MatDividerModule,
    MatSliderModule, MatSelectModule, MatButtonModule, InputCvaComponent,
  ],
  templateUrl: './filter-bottom-sheet.component.html',
  styleUrl: './filter-bottom-sheet.component.scss'
})
export class FilterBottomSheetComponent {
  memberParams: MemberParams | undefined;
  minAge = 18;
  maxAge = 99;
  private _fb = inject(FormBuilder);
  //#region Reactive Form
  filterFg = this._fb.group({
    userNameOrKnownAs: ['', []],
    genderCtrl: [],
    minAgeCtrl: [this.minAge],
    maxAgeCtrl: [this.maxAge],
  });
  private _memberService = inject(MemberService);

  constructor() {
    this.memberParams = this._memberService.memberParams;
    console.log(this.memberParams);
  }

  get UserNameOrKnownAs(): FormControl {
    return this.filterFg.get('userNameOrKnownAs') as FormControl;
  }

  get GenderCtrl(): AbstractControl {
    return this.filterFg.get('genderCtrl') as FormControl;
  }

  get MinAgeCtrl(): AbstractControl {
    return this.filterFg.get('minAgeCtrl') as FormControl;
  }

  get MaxAgeCtrl(): AbstractControl {
    return this.filterFg.get('maxAgeCtrl') as FormControl;
  }

  //#endregion Reactive form

  updateMemberParams(): void {
    if (this.memberParams) {
      this.memberParams.userNameOrKnownAs = this.UserNameOrKnownAs.value;
      this.memberParams.gender = this.GenderCtrl.value;
      this.memberParams.minAge = this.MinAgeCtrl.value;
      this.memberParams.maxAge = this.MaxAgeCtrl.value;
    }

    this._memberService.eventEmitOrderFilterBottomSheet.emit();
  }

  disableButton(): boolean {
    return (this.UserNameOrKnownAs.value.length < 1 || this.UserNameOrKnownAs.pristine) &&
      (this.GenderCtrl.value === this.memberParams?.gender || this.GenderCtrl.pristine) &&
      this.MinAgeCtrl.value === this.memberParams?.minAge &&
      this.MaxAgeCtrl.value === this.memberParams?.maxAge
  }
}
