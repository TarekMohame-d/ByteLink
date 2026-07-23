import { useMutation } from "@tanstack/react-query";
import type { ApiError } from "@/types/api";
import {
  type ResendVerificationPayload,
  resendVerificationCommand,
  type VerifyEmailPayload,
  verifyEmailCommand,
} from "../api/verify";

export function useVerifyEmail() {
  return useMutation<void, ApiError, VerifyEmailPayload>({
    mutationFn: async (payload) => {
      const result = await verifyEmailCommand(payload);
      if (!result.success) {
        throw result.error;
      }
      return result.value;
    },
  });
}

export function useResendVerification() {
  return useMutation<void, ApiError, ResendVerificationPayload>({
    mutationFn: async (payload) => {
      const result = await resendVerificationCommand(payload);
      if (!result.success) {
        throw result.error;
      }
      return result.value;
    },
  });
}
