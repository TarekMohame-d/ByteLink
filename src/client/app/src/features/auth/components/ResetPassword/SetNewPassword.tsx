import { Link, useSearch } from "@tanstack/react-router";
import { Eye, EyeOff, LinkIcon, MoveLeft, Shield } from "lucide-react";
import { useCallback, useState } from "react";
import z from "zod";
import { FieldGroup } from "@/components/ui/field";
import { useResetPassword } from "../../hooks/use-reset-password";
import { AuthCard } from "../AuthCard";
import MessageContainer from "../MessageContainer";

interface SetNewPasswordProps {
  setStatus: (status: "email" | "password" | "success") => void;
}

interface FormErrors {
  confirmPassword?: string;
  password?: string;
}

const SetNewPasswordFormSchema = z
  .object({
    confirmPassword: z.string(),
    password: z.string().regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/, {
      message:
        "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number",
    }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

function SetNewPassword({ setStatus }: SetNewPasswordProps) {
  const search = useSearch({ from: "/auth/reset-password" });

  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [fieldErrors, setFieldErrors] = useState<FormErrors>({});

  const {
    mutate: resetPassword,
    isPending,
    error: mutationError,
    reset: resetMutation,
  } = useResetPassword();

  const handlePasswordChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setPassword(e.target.value);
      if (fieldErrors.password) {
        setFieldErrors((prev) => ({ ...prev, password: undefined }));
      }
      if (mutationError) {
        resetMutation();
      }
    },
    [fieldErrors.password, mutationError, resetMutation]
  );

  const handleConfirmPasswordChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      setConfirmPassword(e.target.value);
      if (fieldErrors.confirmPassword) {
        setFieldErrors((prev) => ({ ...prev, confirmPassword: undefined }));
      }
      if (mutationError) {
        resetMutation();
      }
    },
    [fieldErrors.confirmPassword, mutationError, resetMutation]
  );

  const validateForm = useCallback(() => {
    const validation = SetNewPasswordFormSchema.safeParse({
      confirmPassword,
      password,
    });
    if (!validation.success) {
      const errors: FormErrors = {};
      for (const issue of validation.error.issues) {
        const field = issue.path[0] as keyof FormErrors;
        if (field && !errors[field]) {
          errors[field] = issue.message;
        }
      }
      setFieldErrors(errors);
      return false;
    }
    return true;
  }, [confirmPassword, password]);

  const handleSubmit = useCallback(
    (e: React.SubmitEvent<HTMLFormElement>) => {
      e.preventDefault();
      e.stopPropagation();
      setFieldErrors({});
      resetMutation();

      if (!validateForm()) {
        return;
      }

      resetPassword(
        {
          password,
          token: (search.token as string) || "",
        },
        {
          onError: (err) => {
            if (err.errors) {
              const serverErrors: FormErrors = {};
              for (const [serverFieldName, messages] of Object.entries(
                err.errors
              )) {
                const clientFieldName = (serverFieldName
                  .charAt(0)
                  .toLowerCase() +
                  serverFieldName.slice(1)) as keyof FormErrors;

                if (messages && messages.length > 0) {
                  serverErrors[clientFieldName] = messages.join(", ");
                }
              }
              setFieldErrors(serverErrors);
            }
          },
          onSuccess: () => {
            setStatus("success");
          },
        }
      );
    },
    [
      resetPassword,
      password,
      resetMutation,
      search.token,
      setStatus,
      validateForm,
    ]
  );

  const handlePasswordShow = useCallback(() => {
    setShowPassword((prev) => !prev);
  }, []);

  const handleConfirmPasswordShow = useCallback(() => {
    setShowConfirmPassword((prev) => !prev);
  }, []);

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
          <Shield size={40} />
        </div>

        <div className="space-y-2 text-center">
          <h1 className="font-bold text-2xl text-gray-900 md:text-3xl">
            Set New Password
          </h1>
          <p className="text-gray-500">
            Please set a new password for your account
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
                htmlFor="password"
              >
                Password
              </label>
              <div className="relative">
                <input
                  className="w-full rounded-md border border-gray-300 px-3 py-2 pr-10 text-gray-900 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  id="password"
                  onChange={handlePasswordChange}
                  placeholder="Password..."
                  type={showPassword ? "text" : "password"}
                  value={password}
                />
                <button
                  aria-label={showPassword ? "Hide password" : "Show password"}
                  className="absolute inset-y-0 right-0 pr-3"
                  onClick={handlePasswordShow}
                  type="button"
                >
                  {showPassword ? (
                    <EyeOff className="h-4 w-4 text-indigo-600" />
                  ) : (
                    <Eye className="h-4 w-4 text-indigo-600" />
                  )}
                </button>
              </div>
              {!!fieldErrors.password && (
                <span className="text-red-500 text-xs">
                  {fieldErrors.password}
                </span>
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <label
                className="font-medium text-gray-700 text-sm"
                htmlFor="confirm-password"
              >
                Confirm Password
              </label>
              <div className="relative">
                <input
                  className="w-full rounded-md border border-gray-300 px-3 py-2 pr-10 text-gray-900 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                  id="confirm-password"
                  onChange={handleConfirmPasswordChange}
                  placeholder="Confirm Password..."
                  type={showConfirmPassword ? "text" : "password"}
                  value={confirmPassword}
                />
                <button
                  aria-label={
                    showConfirmPassword
                      ? "Hide confirm password"
                      : "Show confirm password"
                  }
                  className="absolute inset-y-0 right-0 pr-3"
                  onClick={handleConfirmPasswordShow}
                  type="button"
                >
                  {showConfirmPassword ? (
                    <EyeOff className="h-4 w-4 text-indigo-600" />
                  ) : (
                    <Eye className="h-4 w-4 text-indigo-600" />
                  )}
                </button>
              </div>
              {!!fieldErrors.confirmPassword && (
                <span className="text-red-500 text-xs">
                  {fieldErrors.confirmPassword}
                </span>
              )}
            </div>

            <button
              className="rounded-md bg-indigo-600 px-4 py-2 font-semibold text-white hover:bg-indigo-500 disabled:cursor-not-allowed disabled:bg-indigo-400"
              disabled={isPending}
              type="submit"
            >
              {isPending ? "Setting Password..." : "Set New Password"}
            </button>
          </FieldGroup>
        </form>
      </AuthCard.Body>

      <AuthCard.Footer>
        <div className="flex flex-col items-center">
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

export default SetNewPassword;
