import { Link, useRouter } from "@tanstack/react-router";
import { Link as LinkIcon } from "lucide-react";
import { useCallback, useState } from "react";
import z from "zod";
import BackgroundGradient from "@/components/BackgroundGradient";
import { FieldGroup } from "@/components/ui/field";
import { useSignUp } from "../hooks/use-auth";
import { BrandCover } from "./BrandCover";
import { EmailInput } from "./form/EmailInput";
import { PasswordInput } from "./form/PasswordInput";
import { SubmitButton } from "./form/SubmitButton";
import { TextInput } from "./form/TextInput";
import MessageContainer from "./MessageContainer";

interface FieldErrors {
  confirmPassword?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  password?: string;
}

const SignUpFormSchema = z
  .object({
    confirmPassword: z.string(),
    email: z.email("Invalid email address"),
    firstName: z
      .string()
      .max(25, "First name is too long")
      .min(2, "First name is too short"),
    lastName: z
      .string()
      .max(25, "Last name is too long")
      .min(2, "Last name is too short"),
    password: z.string().regex(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/, {
      message:
        "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number",
    }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords do not match",
    path: ["confirmPassword"],
  });

const INITIAL_FORM_DATA = {
  confirmPassword: "",
  email: "",
  firstName: "",
  lastName: "",
  password: "",
};

export default function SignUpForm() {
  const router = useRouter();

  const [formData, setFormData] = useState(INITIAL_FORM_DATA);
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});

  const {
    mutate: signUp,
    isPending,
    error: mutationError,
    reset: resetMutation,
  } = useSignUp();

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

  const handleSubmit = useCallback(
    (e: React.SubmitEvent<HTMLFormElement>) => {
      e.preventDefault();
      e.stopPropagation();
      setFieldErrors({});
      resetMutation();

      const validation = SignUpFormSchema.safeParse(formData);

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

      signUp(
        {
          email: formData.email,
          firstName: formData.firstName,
          lastName: formData.lastName,
          password: formData.password,
        },
        {
          onError: (err) => {
            if (err.errors) {
              const serverErrors: FieldErrors = {};
              for (const [serverFieldName, messages] of Object.entries(
                err.errors
              )) {
                const clientFieldName = (serverFieldName
                  .charAt(0)
                  .toLowerCase() +
                  serverFieldName.slice(1)) as keyof FieldErrors;

                if (messages && messages.length > 0) {
                  serverErrors[clientFieldName] = messages.join(", ");
                }
              }
              setFieldErrors(serverErrors);
            }
          },
          onSuccess: () => {
            router.navigate({
              search: { email: formData.email },
              to: "/auth/verify-email",
            });
          },
        }
      );
    },
    [formData, resetMutation, router, signUp]
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
            <p className="font-bold text-2xl">
              Shrink your links. Grow your reach.
            </p>
            <p className="text-gray-300 text-sm">
              Turn messy URLs into clean, clickable assets in seconds.
            </p>
          </BrandCover.Body>
          <BrandCover.Footer>
            <p className="text-gray-400 text-xs">
              &copy; {new Date().getFullYear()} ByteLink. All rights reserved.
            </p>
          </BrandCover.Footer>
        </BrandCover>

        {/* Form */}
        <div className="col-span-2 flex flex-1 flex-col items-center justify-center pt-6 pb-6">
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

              {/* First Name */}
              <TextInput
                error={fieldErrors.firstName}
                id="firstName"
                label="First Name"
                name="firstName"
                onChange={handleInputChange}
                placeholder="First Name..."
                value={formData.firstName}
              />

              {/* Last Name */}
              <TextInput
                error={fieldErrors.lastName}
                id="lastName"
                label="Last Name"
                name="lastName"
                onChange={handleInputChange}
                placeholder="Last Name..."
                value={formData.lastName}
              />

              {/* Email */}
              <EmailInput
                error={fieldErrors.email}
                id="email"
                label="Email Address"
                name="email"
                onChange={handleInputChange}
                placeholder="name@example.com"
                value={formData.email}
              />

              {/* Password */}
              <PasswordInput
                error={fieldErrors.password}
                id="password"
                label="Password"
                name="password"
                onChange={handleInputChange}
                placeholder="Password..."
                value={formData.password}
              />

              {/* Confirm Password */}
              <PasswordInput
                error={fieldErrors.confirmPassword}
                id="confirmPassword"
                label="Confirm Password"
                name="confirmPassword"
                onChange={handleInputChange}
                placeholder="Confirm Password..."
                value={formData.confirmPassword}
              />

              {/* Submit Button */}
              <SubmitButton
                className="w-full"
                disabled={isPending}
                label="Sign Up"
              />

              <p className="flex justify-center gap-0.5 text-gray-950 text-sm">
                Already have an account?
                <Link
                  className="font-semibold text-indigo-600 hover:underline"
                  to="/auth/sign-in"
                >
                  Sign In
                </Link>
              </p>
            </FieldGroup>
          </form>
        </div>
      </div>
    </div>
  );
}
