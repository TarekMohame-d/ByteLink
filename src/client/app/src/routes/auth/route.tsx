import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/auth")({
  beforeLoad: ({ location, context }) => {
    const { isAuthenticated } = context.auth.getState();

    if (isAuthenticated) {
      throw redirect({ to: "/" });
    }

    if (location.pathname === "/auth" || location.pathname === "/auth/") {
      throw redirect({ to: "/auth/sign-in" });
    }
  },
  component: AuthLayout,
});

function AuthLayout() {
  return <Outlet />;
}
