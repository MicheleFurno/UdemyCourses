import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { User } from '../_models/user';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {

  private http = inject(HttpClient)
  baseUrl='https://localhost:5001/api/';
  currentUser = signal<User | null>(null);

  login(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', model)
                    .pipe(
                            map(response => {
                              if(response) { localStorage.setItem('user', JSON.stringify(response)); 
                                             this.currentUser.set(response)
                                           }
                                           }
                                )
                          );
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

  register(model: any) {
    return this.http.post<User>(this.baseUrl + 'account/register', model)
                    .pipe(
                            map(response => {
                              if(response) 
                                { localStorage.setItem('user', JSON.stringify(response)); 
                                  this.currentUser.set(response);
                                }
                              return response;
                            })
                      );
  }
}
