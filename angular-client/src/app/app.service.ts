import { Injectable, Injector } from '@angular/core';
import { catchError, tap } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { AuthService } from './auth/auth.service';


@Injectable({
  providedIn: 'root'
})
export class AppService {

  private authService: AuthService;
  
  constructor(private injector: Injector) { }

  init(): Promise<void> {
    return new Promise<void>((resolve) => {

      this.authService = this.injector.get(AuthService);

      this.authService.tryLogin().pipe(
        catchError(error => {
          return throwError(error);
        })
      )
      .subscribe(
        () => resolve(),
        error => {
          resolve();
        });
    });
  }
}