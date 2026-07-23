import { useMutation } from "@tanstack/react-query";
import type { ApiError } from "@/types/api";
import {
  type RequestResetPasswordPayload,
  type ResetPasswordPayload,
  requestResetPasswordCommand,
  resetPasswordCommand,
} from "../api/reset-password";

export function useRequestResetPassword() {
  return useMutation<void, ApiError, RequestResetPasswordPayload>({
    mutationFn: async (payload) => {
      const result = await requestResetPasswordCommand(payload);
      if (!result.success) {
        throw result.error;
      }
      return result.value;
    },
  });
}

export function useResetPassword() {
  return useMutation<void, ApiError, ResetPasswordPayload>({
    mutationFn: async (payload) => {
      const result = await resetPasswordCommand(payload);
      if (!result.success) {
        throw result.error;
      }
      return result.value;
    },
  });
}
