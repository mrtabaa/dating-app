import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { AfterViewChecked, Component, Input, ViewChild } from '@angular/core';
import { Message } from '../../../../models/message.model';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Member } from '../../../../models/member.model';

@Component({
  selector: 'app-cdk-virtual-scroller',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage,
    ScrollingModule
  ],
  templateUrl: './cdk-virtual-scroller.component.html',
  styleUrl: './cdk-virtual-scroller.component.scss'
})
export class CdkVirtualScrollerComponent implements AfterViewChecked {
  ngAfterViewChecked(): void {
    if (this.messagesIn)
      console.log(this.messagesIn);
  }
  @ViewChild(CdkVirtualScrollViewport) private viewport: CdkVirtualScrollViewport | undefined;
  @Input() messagesIn: Message[] | undefined;
  @Input() memberIn: Member | undefined;
  photo_WH = 40;
  bufferSize = 0;

  loadOlderMessages(event: number): void {
    if (this.viewport) {
      const range = this.viewport.getRenderedRange();

      if (event === range.start) {
        console.log('Scrolled:', event);
        // this.messageParams.pageNumber++;
        // this.getMessages();
        // this.scrollToReloaded();
      }
    }
  }

  // TODO remove if not used
  // scrollToReloaded() {
  //   try {
  //     setTimeout(() => {
  //       if (this.viewport) {
  //         this.viewport.scrollToIndex((this.messages.length) - this.messageParams.pageSize * (this.messageParams.pageNumber - 1), 'instant');
  //       }
  //     }, 0);
  //   } catch (err) { console.error(err) }
  // }
}
