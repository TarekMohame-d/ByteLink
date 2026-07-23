import { createFileRoute } from "@tanstack/react-router";
import z from "zod";
import VerifyEmail from "@/features/auth/components/VerifyEmail/VerifyEmail";

const searchSchema = z.object({
  email: z.email(),
  token: z.string().optional(),
});

export const Route = createFileRoute("/auth/verify-email")({
  component: RouteComponent,
  validateSearch: searchSchema,
});

function RouteComponent() {
  return <VerifyEmail />;
}
