import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'src/environments/environment';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  template: `
    <div style="text-align:center" class="content">
      <h1>
        App!
      </h1>
      <h2>Login ok: {{ loginOk }}</h2>
      <h2>Api ok: {{ apiOk }}</h2>
      <h3><a *ngIf="!loginOk" href="#" (click)="onLogin($event)">Login</a></h3>
      <h3><a *ngIf="loginOk" href="#" (click)="onLogout($event)">Logout</a></h3>
    </div>
  `,
  styles: []
})
export class AppComponent implements OnInit {
  loginOk = false;
  apiOk = false;

  constructor(private http: HttpClient, private authService: AuthService) { }

  ngOnInit() {
    this.loginOk = this.authService.isLogged();

    if(this.loginOk) {
      const headers = new HttpHeaders({
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${this.authService.getAccessToken()}`
      })
      this.http.get<any>(`${environment.apiBaseUrl}/api/test`, { headers: headers }).subscribe(() => this.apiOk = true);
    }
  }

  onLogout($event: MouseEvent) {
    $event.preventDefault();
    this.authService.logout();
  }

  onLogin($event: MouseEvent) {
    $event.preventDefault();
    return this.authService.login();
  }
}
