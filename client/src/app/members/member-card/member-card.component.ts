import { Component, computed, inject, input } from '@angular/core';
import { Member } from '../../_models/member';
import { RouterLink } from '@angular/router';
import { LikesService } from '../../_services/likes.service';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-member-card',
  standalone: true,
  imports: [RouterLink, NgIf],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {

  private likeService = inject(LikesService);
  hasLiked = computed(() => this.likeService.likeIds().includes(this.member().id));

  member = input.required<Member>();

  toggleLike() {
    this.likeService.toggleLike(this.member().id).subscribe({
      next: () => {
        if(this.hasLiked()) this.likeService.likeIds.update(ids => ids.filter(l => l !== this.member().id));
        else this.likeService.likeIds.update(ids => [...ids, this.member().id]);
      }
    });
  }

}
