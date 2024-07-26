import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Message } from '../../../../models/message.model';
import { ShortenStringPipe } from '../../../../pipes/shorten-string.pipe';
import { ResponsiveService } from '../../../../services/responsive.service';

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
  @Input() pageSizeIn: number | undefined;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  photo_WH = 40;
  defaultItemSize = 50;
  bufferSize = 0;

  ngOnInit(): void {
    if (this.pageSizeIn)
      this.bufferSize = this.defaultItemSize * this.pageSizeIn
  }

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
