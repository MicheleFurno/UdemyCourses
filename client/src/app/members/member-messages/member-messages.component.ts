import { Component, inject, input, OnInit, output, ViewChild } from '@angular/core';
import { Message } from '../../_models/message';
import { TimeagoModule } from 'ngx-timeago';
import { MessagesService } from '../../_services/messages.service';
import { FormsModule, NgForm } from '@angular/forms';

@Component({
  selector: 'app-member-messages',
  standalone: true,
  imports: [TimeagoModule, FormsModule],
  templateUrl: './member-messages.component.html',
  styleUrl: './member-messages.component.css'
})
export class MemberMessagesComponent {
  @ViewChild('messageForm') messageForm?: NgForm;
  private messageService = inject(MessagesService)
  userName = input.required<string>();
  messages = input.required<Message[]>();
  messageContent = "";
  updateMessage = output<Message>();

  sendMessage() {
    this.messageService.sendMessage(this.userName(), this.messageContent).subscribe({
      next: message => {
        this.updateMessage.emit(message);
        this.messageForm?.reset();
      }
    })
  }

}
