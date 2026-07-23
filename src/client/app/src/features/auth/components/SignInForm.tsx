import {
  Link,
  useRouteContext,
  useRouter,
  useSearch,
} from "@tanstack/react-router";
import { Link as LinkIcon } from "lucide-react";
import { useCallback, useState } from "react";
import z from "zod";
import BackgroundGradient from "@/components/BackgroundGradient";
import { FieldGroup } from "@/components/ui/field";
import { useSignIn } from "../hooks/use-auth";
import { BrandCover } from "./BrandCover";
import { EmailInput } from "./form/EmailInput";
import { PasswordInput } from "./form/PasswordInput";
import { SubmitButton } from "./form/SubmitButton";
import MessageContainer from "./MessageContainer";

const SignInFormSchema = z.object({
  email: z.email("Invalid email address"),
  password: z.string().min(1, "Password is required"),
});

interface FieldErrors {
  email?: string;
  password?: string;
}

export default function SignInForm() {
  const router = useRouter();
  const search = useSearch({ from: "/auth/sign-in" });

  const authStore = useRouteContext({
    from: "/auth/sign-in",
    select: (context) => context.auth.getState(),
  });

  const INITIAL_FORM_DATA = {
    email: "",
    password: "",
  };

  const [formData, setFormData] = useState(INITIAL_FORM_DATA);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  const {
    mutate: signIn,
    isPending,
    error: mutationError,
    reset: resetMutation,
  } = useSignIn();

  const handleSubmit = useCallback(
    (e: React.SubmitEvent<HTMLFormElement>) => {
      e.preventDefault();
      e.stopPropagation();
      setFieldErrors({});
      resetMutation();

      const validation = SignInFormSchema.safeParse(formData);
      if (!validation.success) {
        const errors: FieldErrors = {};
        for (const issue of validation.error.issues) {
          const field = issue.path[0] as keyof FieldErrors;
          if (field && !errors[field]) {
            errors[field] = issue.message;
          }
        }
        setFieldErrors(errors);
        return;
      }

      signIn(
        { email: formData.email, password: formData.password },
        {
          onError: (err) => {
            if (err.errorCode === "EmailNotVerified") {
              router.navigate({
                search: { email: formData.email },
                to: "/auth/verify-email",
              });
            }
          },
          onSuccess: async () => {
            await authStore.fetchMe();
            const targetDestination = search.redirect || "/";
            router.navigate({ to: targetDestination });
          },
        }
      );
    },
    [authStore, resetMutation, router, search.redirect, signIn, formData]
  );

  const handleInputChange = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const { name, value } = e.target;

      setFormData((prev) => ({ ...prev, [name]: value }));

      setFieldErrors((prev) => {
        if (prev[name as keyof FieldErrors]) {
          return { ...prev, [name]: undefined };
        }
        return prev;
      });

      if (mutationError) {
        resetMutation();
      }
    },
    [mutationError, resetMutation]
  );

  const formError = mutationError
    ? mutationError.errorDescription || mutationError.message
    : null;

  return (
    <div className="relative flex min-h-screen flex-col items-center justify-center p-4 md:p-12">
      <BackgroundGradient />
      <div className="flex w-full flex-1 flex-col justify-center overflow-hidden rounded-2xl border border-gray-200 bg-white shadow-2xl md:grid md:w-4/5 md:grid-cols-3">
        <BrandCover>
          <BrandCover.Header>
            <div className="flex h-8 w-8 items-center justify-center rounded-sm border-2 border-white/60 bg-indigo-50/20">
              <LinkIcon color="#ffffff" />
            </div>
            <span className="translate-y-0.5 font-chains text-4xl leading-none">
              BYTELINK
            </span>
          </BrandCover.Header>
          <BrandCover.Body>
            <p className="font-bold text-2xl">Ready to trim some more URLs?</p>
            <p className="text-gray-300 text-sm">
              Sign in to view your dashboard and manage your active links.
            </p>
          </BrandCover.Body>
          <BrandCover.Footer>
            <p className="text-gray-400 text-xs">
              &copy; {new Date().getFullYear()} ByteLink. All rights reserved.
            </p>
          </BrandCover.Footer>
        </BrandCover>

        {/* Form */}
        <div className="col-span-2 flex flex-1 flex-col items-center justify-center">
          <div className="mb-18 flex flex-row gap-3 md:hidden">
            <div className="flex h-8 w-8 items-center justify-center rounded-sm border border-gray-400 bg-indigo-600">
              <LinkIcon color="#ffffff" />
            </div>
            <span className="translate-y-0.5 font-chains text-4xl text-indigo-600 leading-none">
              BYTELINK
            </span>
          </div>

          <form className="w-full p-2 md:w-1/2 md:p-0" onSubmit={handleSubmit}>
            <FieldGroup>
              {!!formError && (
                <MessageContainer message={formError} type="error" />
              )}

              {/* Email Input */}
              <EmailInput
                error={fieldErrors.email}
                id="email"
                label="Email Address"
                name="email"
                onChange={handleInputChange}
                placeholder="name@example.com"
                value={formData.email}
              />

              {/* Password Input */}
              <div className="flex flex-col gap-2">
                <PasswordInput
                  error={fieldErrors.password}
                  id="password"
                  label="Password"
                  name="password"
                  onChange={handleInputChange}
                  placeholder="Password..."
                  value={formData.password}
                />

                <Link
                  className="text-right font-semibold text-indigo-600 text-xs hover:underline"
                  to="/auth/reset-password"
                >
                  Forgot Your Password?
                </Link>
              </div>

              {/* Submit Button */}
              <SubmitButton
                className="w-full"
                disabled={isPending}
                label="Sign In"
              />

              <p className="flex justify-center gap-0.5 text-gray-950 text-sm">
                Don&apos;t have an account?
                <Link
                  className="font-semibold text-indigo-600 hover:underline"
                  to="/auth/sign-up"
                >
                  Sign Up
                </Link>
              </p>
            </FieldGroup>
          </form>
        </div>
      </div>
    </div>
  );
}
