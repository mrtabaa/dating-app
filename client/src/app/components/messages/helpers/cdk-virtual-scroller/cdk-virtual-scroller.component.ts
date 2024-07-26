import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Message } from '../../../../models/message.model';
import { ShortenStringPipe } from '../../../../pipes/shorten-string.pipe';
import { ResponsiveService } from '../../../../services/responsive.service';
import { MessagesMobileComponent } from '../../messages-mobile/messages-mobile.component';

@Component({
  selector: 'app-cdk-virtual-scroller',
  standalone: true,
  imports: [
    CommonModule, NgOptimizedImage,
    ScrollingModule, ShortenStringPipe
  ],
  templateUrl: './cdk-virtual-scroller.component.html',
  styleUrl: './cdk-virtual-scroller.component.scss'
})
export class CdkVirtualScrollerComponent implements OnInit {
  @ViewChild(CdkVirtualScrollViewport) private viewport: CdkVirtualScrollViewport | undefined;
  @Input() messagesIn: Message[] | undefined;
  private _messagesMobileComponent = inject(MessagesMobileComponent)
  private _messageParams = this._messagesMobileComponent.messageParams;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  photo_WH = 40;
  defaultItemSize = 50;
  bufferSize = 0;

  ngOnInit(): void {
    this.bufferSize = this.defaultItemSize * this._messageParams.pageSize
  }

  loadOlderMessages(event: number): void {
    console.log(this.viewport);
    if (this.viewport) {
      const range = this.viewport.getRenderedRange();

      if (event === range.end) {
        console.log('Scrolled:', event);
        this._messageParams.pageNumber++;
        this._messagesMobileComponent.getMessages();
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
