import { ApplicationConfig, importProvidersFrom } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { provideToastr } from 'ngx-toastr';

import { routes } from './app.routes';
import { JwtModule } from '@auth0/angular-jwt';

export function tokenGetter() {
  return localStorage.getItem("token");
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideAnimations(),
    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-top-right',
      preventDuplicates: true,
      closeButton: true,
      progressBar: true,
    }),
    importProvidersFrom(
      JwtModule.forRoot({
        config: {
          tokenGetter: tokenGetter,
          allowedDomains: ["localhost:53812"], // Should match backend, but using proxy covers this if relative
        },
      })
    ),
  ]
};
