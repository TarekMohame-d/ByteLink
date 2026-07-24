import { safeRequest } from "@/lib/api-client";
import type { ApiError } from "@/types/api";
import type { Result } from "@/types/result";

export interface SignInPayload {
  email: string;
  password: string;
}

export interface SignUpPayload {
  email: string;
  firstName: string;
  lastName: string;
  password: string;
}

export async function signInCommand(
  payload: SignInPayload
): Promise<Result<void, ApiError>> {
  return await safeRequest({
    data: payload,
    method: "POST",
    url: "/api/v1/identity/login",
  });
}

export async function signUpCommand(
  payload: SignUpPayload
): Promise<Result<void, ApiError>> {
  return await safeRequest({
    data: payload,
    method: "POST",
    url: "/api/v1/identity/register",
  });
}
