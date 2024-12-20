import { CdkVirtualScrollViewport, ScrollingModule } from '@angular/cdk/scrolling';
import { Component, inject, Input, OnInit, ViewChild } from '@angular/core';
import { CommonModule, NgOptimizedImage } from '@angular/common';
import { Message } from '../../../../models/message.model';
import { ShortenStringPipe } from '../../../../pipes/shorten-string.pipe';
import { ResponsiveService } from '../../../../services/responsive.service';
import { MessagesMobileComponent } from '../../messages-mobile/messages-mobile.component';
import { MemberDetailTabs } from '../../../../enums/member-detail-tabs.enum';
import { RouterModule } from '@angular/router';

@Component({
    selector: 'app-cdk-virtual-scroller',
    imports: [
        CommonModule, NgOptimizedImage, RouterModule,
        ScrollingModule, ShortenStringPipe
    ],
    templateUrl: './cdk-virtual-scroller.component.html',
    styleUrl: './cdk-virtual-scroller.component.scss'
})
export class CdkVirtualScrollerComponent implements OnInit {
  @ViewChild(CdkVirtualScrollViewport) private viewport: CdkVirtualScrollViewport | undefined;
  @Input() messagesIn: Message[] | undefined;
  @Input() totalItemsCountIn: number | undefined;
  private _messagesMobileComponent = inject(MessagesMobileComponent)
  private _messageParams = this._messagesMobileComponent.messageParams;
  isMobileSig = inject(ResponsiveService).isMobileSig;
  photo_WH = 40;
  defaultItemSize = 50;
  bufferSize = 0;
  isFirstLoad = true;
  MemberDetailTabs = MemberDetailTabs;

  ngOnInit(): void {
    this.bufferSize = this.defaultItemSize * this._messageParams.pageSize
  }

  loadOlderMessages(event: number): void {
    if (this.viewport) {
      const range = this.viewport.getRenderedRange();
      event += 7; // adjust event to match the range.end. Event is init 7 items less than actual range.end

      if (event === range?.end && this.messagesIn?.length !== this.totalItemsCountIn) {
        this._messageParams.pageNumber++;
        this._messagesMobileComponent.getMessages();

        // scroll to index 0 on first load
        if (this.isFirstLoad) {
          this.viewport.scrollToIndex(0);
          this.isFirstLoad = false;
        }
      }
    }
  }
}
