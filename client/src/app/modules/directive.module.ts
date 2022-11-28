import { NgModule } from '@angular/core';
import { ClearControlFieldByClickDirective } from '../_directives/clear-control-field-by-click.directive';
import { InputFormatDirective } from '../_directives/input-format.directive';
import { LazyImgDirective } from '../_directives/lazy-img.directive';
import { AutofocusDirective } from '../_directives/autofocus.directive';

const directives: any[] = [
  InputFormatDirective,
  ClearControlFieldByClickDirective,
  LazyImgDirective,
  AutofocusDirective,
]

@NgModule({
  declarations: [directives],
  exports: [directives]
})
export class DirectiveModule { }
