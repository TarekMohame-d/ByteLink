import { createFileRoute } from "@tanstack/react-router";
import z from "zod";
import SignInForm from "@/features/auth/components/SignInForm";

const signInSearchSchema = z.object({
  email: z.email().optional(),
  redirect: z.string().optional(),
});

export const Route = createFileRoute("/auth/sign-in")({
  component: SignInRouteComponent,
  validateSearch: signInSearchSchema,
});

function SignInRouteComponent() {
  return <SignInForm />;
}
