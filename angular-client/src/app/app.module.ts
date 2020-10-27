import { BrowserModule } from '@angular/platform-browser';
import { APP_INITIALIZER, NgModule, Provider } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { AppService } from './app.service';

function appServiceInit(startupService: AppService): Function {
  return (): Promise<any> => {
    return startupService.init();
  }
}

const STARTUP: Provider = {
  provide: APP_INITIALIZER,
  useFactory: appServiceInit,
  deps: [AppService],
  multi: true
};

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule
  ],
  providers: [
    STARTUP
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
