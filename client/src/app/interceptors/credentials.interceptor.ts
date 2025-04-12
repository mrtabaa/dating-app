import {HttpInterceptorFn} from '@angular/common/http';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const reqWithCreds = req.clone({withCredentials: true});
  return next(reqWithCreds);
};
