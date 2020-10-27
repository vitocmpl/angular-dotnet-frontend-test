import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private access_token: string;

  constructor(private http: HttpClient) { }

  tryLogin() {
    return this.http.get<{ accessToken: string }>(`${environment.frontendBaseUrl}/api/auth`, { withCredentials: true }).pipe(
      tap(res => this.access_token = res.accessToken)
    );
  }

  login() {
    window.location.href = `${environment.frontendBaseUrl}/api/auth/login`;
  }

  logout() {
    this.access_token = null;
    window.location.href = `${environment.frontendBaseUrl}/api/auth/logout`;
  }

  isLogged() {
    return !!this.access_token;
  }
  
  getAccessToken() {
    return this.access_token;
  }
}
