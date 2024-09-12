import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_services/account.service';
import { BsDropdownModule} from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TitleCasePipe } from '@angular/common';

@Component({
  selector: 'app-nav',
  standalone: true,
  imports: [FormsModule, BsDropdownModule, RouterLink, RouterLinkActive, TitleCasePipe],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css'
})
export class NavComponent {

  private router = inject(Router)
  private toastr = inject(ToastrService)
  accountService=inject(AccountService);
  model: any = {}

  login() {
    this.accountService.login(this.model).subscribe({
      next: () => { 
        void this.router.navigateByUrl('/members'); 
        this.model = {} 
      },
      error: error => this.toastr.error(error.error)
    })
  }

  logout() {
    this.accountService.logout();
    void this.router.navigateByUrl('/');
  }

}
