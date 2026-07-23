import { createFileRoute } from "@tanstack/react-router";
import SignUpForm from "../../features/auth/components/SignUpForm";

export const Route = createFileRoute("/auth/sign-up")({
  component: SignUpRouteComponent,
});

function SignUpRouteComponent() {
  return <SignUpForm />;
}
