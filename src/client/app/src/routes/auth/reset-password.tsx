import { createFileRoute } from "@tanstack/react-router";
import z from "zod";
import ResetPassword from "@/features/auth/components/ResetPassword/ResetPassword";

const searchSchema = z.object({
  email: z.email().optional(),
  token: z.string().optional(),
});

export const Route = createFileRoute("/auth/reset-password")({
  component: RouteComponent,
  validateSearch: searchSchema,
});

function RouteComponent() {
  return <ResetPassword />;
}
