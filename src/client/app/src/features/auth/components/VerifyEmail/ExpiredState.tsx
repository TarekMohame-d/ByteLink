import { AlertCircle, LinkIcon } from "lucide-react";
import { useCallback, useState } from "react";
import z from "zod";
import { FieldGroup } from "@/components/ui/field"; // Adjust path as needed
import { useResendVerification } from "../../hooks/use-verify";
import { AuthCard } from "../AuthCard";
import MessageContainer from "../MessageContainer";

const ExpiredFormSchema = z.object({
  email: z.email("Invalid email address"),
});

interface ExpiredStateProps {
  initialEmail: string;
  onSuccess: () => void;
}

export function ExpiredState({ initialEmail, onSuccess }: ExpiredStateProps) {
  const [email, setEmail] = useState(initialEmail);
  const [emailError, setEmailError] = useState<string | null>(null);

  const {
    mutate: resendVerification,
    isPending,
    error: mutationError,
    reset: resetMutation,
  } = useResendVerification();

  const handleSubmit = useCallback(
    (e: React.SubmitEvent<HTMLFormElement>) => {
      e.preventDefault();
      setEmailError(null);
      resetMutation();

      const validation = ExpiredFormSchema.safeParse({ email });
      if (!validation.success) {
        setEmailError(validation.error.issues[0].message);
        return;
      }

      resendVerification(
        { email },
        {
          onSuccess: () => {
            onSuccess();
          },
        }
      );
    },
    [email, onSuccess, resendVerification, resetMutation]
  );

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
        <div className="flex h-20 w-20 items-center justify-center rounded-full bg-amber-50 text-amber-600">
          <AlertCircle size={40} />
        </div>
        <div className="space-y-2 text-center">
          <h1 className="font-bold text-2xl text-gray-900 leading-tight tracking-tight md:text-3xl">
            Link Expired
          </h1>
          <p className="mx-auto max-w-sm text-gray-500">
            This activation token is invalid or expired. Provide your email
            below to receive a new link.
          </p>
        </div>

        <form className="w-full max-w-sm" onSubmit={handleSubmit}>
          <FieldGroup>
            {!!formError && (
              <MessageContainer message={formError} type="error" />
            )}

            <div className="flex flex-col gap-1.5">
              <label
                className="font-medium text-gray-700 text-sm"
                htmlFor="expired-email"
              >
                Email Address
              </label>
              <input
                className="rounded-md border border-gray-300 px-3 py-2 text-gray-900 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                id="expired-email"
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
              disabled={isPending}
              type="submit"
            >
              {isPending ? "Sending Link..." : "Request New Activation Link"}
            </button>
          </FieldGroup>
        </form>
      </AuthCard.Body>
    </AuthCard>
  );
}
