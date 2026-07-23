import { useMutation } from "@tanstack/react-query";
import type { ApiError } from "@/types/api";
import {
  type SignInPayload,
  type SignUpPayload,
  signInCommand,
  signUpCommand,
} from "../api/auth";

export function useSignIn() {
  return useMutation<void, ApiError, SignInPayload>({
    mutationFn: async (payload) => {
      const result = await signInCommand(payload);
      if (!result.success) {
        throw result.error;
      }
      return result.value;
    },
  });
}

export function useSignUp() {
  return useMutation<void, ApiError, SignUpPayload>({
    mutationFn: async (payload) => {
      const result = await signUpCommand(payload);
      if (!result.success) {
        throw result.error;
      }
      return result.value;
    },
  });
}
