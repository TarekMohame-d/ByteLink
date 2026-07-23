import { createFileRoute, Outlet, redirect } from "@tanstack/react-router";

export const Route = createFileRoute("/_main/_authenticated")({
  beforeLoad: async ({ context, location }) => {
    let wasLoggedIn = false;
    try {
      const authSessionRaw = localStorage.getItem("auth_session");
      if (authSessionRaw) {
        const parsedSession = JSON.parse(authSessionRaw);
        wasLoggedIn = Boolean(parsedSession?.state?.wasLoggedIn);
      }
    } catch {
      wasLoggedIn = false;
    }

    if (!wasLoggedIn) {
      throw redirect({
        search: { redirect: location.pathname },
        to: "/auth/sign-in",
      });
    }

    const { isInitialized, fetchMe } = context.auth.getState();
    if (!isInitialized) {
      await fetchMe();
    }

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
