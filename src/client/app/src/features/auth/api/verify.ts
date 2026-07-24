import { safeRequest } from "@/lib/api-client";
import type { ApiError } from "@/types/api";
import type { Result } from "@/types/result";

export interface VerifyEmailPayload {
  token: string;
}

export interface ResendVerificationPayload {
  email: string;
}

export async function verifyEmailCommand(
  payload: VerifyEmailPayload
): Promise<Result<void, ApiError>> {
  return await safeRequest<void>({
    data: payload,
    method: "POST",
    url: "/api/v1/identity/verify-email",
  });
}

export async function resendVerificationCommand(
  payload: ResendVerificationPayload
): Promise<Result<void, ApiError>> {
  return await safeRequest<void>({
    data: payload,
    method: "POST",
    url: "/api/v1/identity/resend-verification",
  });
}
