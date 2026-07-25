import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_main/_authenticated")({
  beforeLoad: ({ context, location }) => {
    const { isAuthenticated } = context.auth.getState();

    if (!isAuthenticated) {
      throw redirect({
        search: { redirect: location.pathname },
        to: "/auth/sign-in",
      });
    }
  },
  component: RouteComponent,
});

function RouteComponent() {
  return <Outlet />;
}
