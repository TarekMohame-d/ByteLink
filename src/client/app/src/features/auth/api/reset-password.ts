import { safeRequest } from "@/lib/api-client";
import type { ApiError } from "@/types/api";
import type { Result } from "@/types/result";

export interface RequestResetPasswordPayload {
  email: string;
}

export interface ResetPasswordPayload {
  password: string;
  token: string;
}

export async function requestResetPasswordCommand(
  payload: RequestResetPasswordPayload
): Promise<Result<void, ApiError>> {
  return await safeRequest<void>({
    data: payload,
    method: "POST",
    url: "/v1/identity/forget-password",
  });
}

export async function resetPasswordCommand(
  payload: ResetPasswordPayload
): Promise<Result<void, ApiError>> {
  return await safeRequest<void>({
    data: payload,
    method: "POST",
    url: "/v1/identity/reset-password",
  });
}
