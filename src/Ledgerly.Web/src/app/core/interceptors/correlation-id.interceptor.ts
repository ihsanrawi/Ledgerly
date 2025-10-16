import { HttpInterceptorFn } from '@angular/common/http';

/**
 * HTTP Interceptor that adds X-Correlation-Id header to all outgoing requests.
 *
 * Per Coding Standards (Rule 4): "Every API request MUST include correlation ID for tracing"
 *
 * Generates a unique UUID v4 for each request to enable distributed tracing and debugging.
 */
export const correlationIdInterceptor: HttpInterceptorFn = (req, next) => {
  // Generate a unique correlation ID for this request using Web Crypto API
  const correlationId = crypto.randomUUID();

  // Clone the request and add the X-Correlation-Id header
  const clonedRequest = req.clone({
    setHeaders: {
      'X-Correlation-Id': correlationId
    }
  });

  return next(clonedRequest);
};
