export interface ApiError {
  detail?: string; // RFC 7807: Detailed explanation (e.g., "Token used or expired.")
  errorCode?: string; // Custom ASP.NET App logic code (e.g., "InvalidToken", "InvalidCredentials")
  errorDescription?: string;
  errors?: Record<string, string[]>; // Validation errors dictionary
  message: string; // The unified, human-readable message we display to users
  status?: number;
  title?: string; // RFC 7807: Summary of error (e.g., "One or more validation errors occurred.")
  traceId?: string; // Server trace identifier for debugging
}
