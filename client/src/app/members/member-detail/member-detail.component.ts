import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { MembersService } from '../../_services/members.service';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../_models/member';
import { TabDirective, TabsetComponent, TabsModule } from 'ngx-bootstrap/tabs'
import { GalleryItem, GalleryModule, ImageItem } from 'ng-gallery'
import { TimeagoModule } from 'ngx-timeago';
import { DatePipe } from '@angular/common';
import { MemberMessagesComponent } from '../member-messages/member-messages.component';
import { Message } from '../../_models/message';
import { MessagesService } from '../../_services/messages.service';

@Component({
  selector: 'app-member-detail',
  standalone: true,
  imports: [TabsModule, GalleryModule, TimeagoModule, DatePipe, MemberMessagesComponent],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit {
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent
  private memberService = inject(MembersService);
  private route = inject(ActivatedRoute);
  private messageService = inject(MessagesService);

  member: Member = {} as Member;
  images: GalleryItem[] = [];
  activeTab?: TabDirective;
  messages: Message[] = [];

  ngOnInit(): void {

    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.member && this.member.photos.map(p => {
          this.images.push(new ImageItem({
            src: p.url, 
            thumb: p.url
          }))
        })        
      }
    });

    this.route.queryParams.subscribe({
      next: params => params['tab'] && this.selectTab(params['tab'])
    });
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    if(this.activeTab.heading === 'Messages' && this.messages.length === 0 && this.member) {
      this.messageService.getMessageThread(this.member.userName).subscribe({
        next: messages => this.messages = messages
      })
    }
  }

  onUpdateMessage(event: Message) {
    this.messages.push(event);
  }

  selectTab(heading: string) {
    if(this.memberTabs) {
      const messageTab = this.memberTabs.tabs.find(t => t.heading === heading);
      if(messageTab) messageTab.active = true;
    }
  }

  /*
  loadMember() {
    const userName = this.route.snapshot.paramMap.get('userName');

    if(!userName) return;

    this.memberService.getMember(userName).subscribe({
      next: member => { 
        this.member = member;
        member.photos.map(p => {
          this.images.push(new ImageItem({
            src: p.url, 
            thumb: p.url
          }))
        })
      }
    })
  }
*/
}
