import { Link } from "@tanstack/react-router";
import { LinkIcon, MoveLeft, RotateCcwKey } from "lucide-react";
import { useCallback, useEffect, useState } from "react";
import z from "zod";
import { FieldGroup } from "@/components/ui/field";
import { useRequestResetPassword } from "../../hooks/use-reset-password";
import { AuthCard } from "../AuthCard";
import MessageContainer from "../MessageContainer";

const ForgotPasswordFormSchema = z.object({
  email: z.email("Invalid email address"),
});

function ForgotPassword() {
  const [email, setEmail] = useState<string>("");
  const [emailError, setEmailError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [countdown, setCountdown] = useState<number>(0);

  const {
    mutate: requestResetPassword,
    isPending,
    error: mutationError,
    reset: resetMutation,
  } = useRequestResetPassword();

  const handleInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setEmail(e.target.value);
      if (emailError) {
        setEmailError(null);
      }
      if (mutationError) {
        resetMutation();
      }
    },
    [emailError, mutationError, resetMutation]
  );

  const handleSubmit = useCallback(
    (e: React.SubmitEvent<HTMLFormElement>) => {
      e.preventDefault();
      e.stopPropagation();
      setEmailError(null);
      setSuccessMessage(null);
      resetMutation();

      const validation = ForgotPasswordFormSchema.safeParse({ email });
      if (!validation.success) {
        setEmailError(validation.error.issues[0].message);
        return;
      }

      requestResetPassword(
        { email },
        {
          onSuccess: () => {
            setSuccessMessage(
              "An email has been sent to you with instructions on how to reset your password."
            );
            setCountdown(60);
          },
        }
      );
    },
    [email, requestResetPassword, resetMutation]
  );

  useEffect(() => {
    if (countdown <= 0) {
      return;
    }

    const timer = setTimeout(() => {
      setCountdown((prev) => prev - 1);
    }, 1000);

    return () => clearTimeout(timer);
  }, [countdown]);

  const formError = mutationError
    ? mutationError.errorDescription || mutationError.message
    : null;

  return (
    <AuthCard>
      <AuthCard.Header>
        <div className="flex h-8 w-8 items-center justify-center rounded-sm border border-gray-500 bg-indigo-600">
          <LinkIcon color="#ffffff" size={16} />
        </div>
        <span className="translate-y-0.5 font-chains text-4xl text-indigo-600 leading-none">
          BYTELINK
        </span>
      </AuthCard.Header>

      <AuthCard.Body>
        <div className="flex h-20 w-20 items-center justify-center rounded-full bg-indigo-50 text-indigo-600">
          <RotateCcwKey size={40} />
        </div>

        <div className="space-y-2 text-center">
          <h1 className="font-bold text-2xl text-gray-900 md:text-3xl">
            Forgot your password?
          </h1>
          <p className="text-gray-500">
            Enter your email address below to receive a password reset link.
          </p>
        </div>

        <form className="w-full max-w-sm" onSubmit={handleSubmit}>
          <FieldGroup>
            {!!formError && (
              <MessageContainer message={formError} type="error" />
            )}

            {!!successMessage && (
              <MessageContainer message={successMessage} type="success" />
            )}

            <div className="flex flex-col gap-1.5">
              <label
                className="font-medium text-gray-700 text-sm"
                htmlFor="email"
              >
                Email Address
              </label>
              <input
                className="rounded-md border border-gray-300 px-3 py-2 text-gray-900 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                id="email"
                onChange={handleInputChange}
                placeholder="name@example.com"
                type="email"
                value={email}
              />
              {!!emailError && (
                <span className="text-red-500 text-xs">{emailError}</span>
              )}
            </div>

            <button
              className="rounded-md bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-500 disabled:cursor-not-allowed disabled:bg-indigo-400"
              disabled={isPending || countdown > 0}
              type="submit"
            >
              {isPending ? "Sending..." : "Send Reset Link"}
            </button>
          </FieldGroup>
        </form>
      </AuthCard.Body>

      <AuthCard.Footer>
        <div className="flex flex-col items-center gap-1.5">
          {countdown > 0 && (
            <p className="flex items-center justify-center gap-1 text-gray-400 text-sm">
              Didn&apos;t get the mail? Resend in{" "}
              <span className="font-bold text-indigo-600">{countdown}s</span>
            </p>
          )}

          <div>
            <p className="flex flex-row items-center gap-2 text-gray-400 text-sm transition-colors hover:text-indigo-500">
              <MoveLeft />
              <Link className="font-semibold" to="/auth/sign-in">
                Return to Sign in
              </Link>
            </p>
          </div>
        </div>
      </AuthCard.Footer>
    </AuthCard>
  );
}

export default ForgotPassword;
