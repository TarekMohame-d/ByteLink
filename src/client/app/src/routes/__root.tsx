import type { QueryClient } from "@tanstack/react-query";
import { createRootRouteWithContext, Outlet } from "@tanstack/react-router";
import React, { Suspense } from "react";
import type { useAuthStore } from "@/store/authStore";

export interface MyRouterContext {
  auth: typeof useAuthStore;
  queryClient: QueryClient;
}

export const Route = createRootRouteWithContext<MyRouterContext>()({
  beforeLoad: async ({ context }) => {
    await context.auth.getState().initializeAuth();
  },
  component: RootComponent,
});

// Lazy-load DevTools ONLY in development
const TanStackDevtools = import.meta.env.DEV
  ? React.lazy(() =>
      Promise.all([
        import("@tanstack/react-devtools"),
        import("@tanstack/react-query-devtools"),
        import("@tanstack/react-router-devtools"),
      ]).then(
        ([
          { TanStackDevtools: TanStackDevtoolsComponent },
          { ReactQueryDevtools },
          { TanStackRouterDevtoolsPanel },
        ]) => ({
          default: () => (
            <TanStackDevtoolsComponent
              config={{ hideUntilHover: false }}
              plugins={[
                {
                  name: "TanStack Query",
                  render: <ReactQueryDevtools />,
                },
                {
                  name: "TanStack Router",
                  render: <TanStackRouterDevtoolsPanel />,
                },
              ]}
            />
          ),
        })
      )
    )
  : () => null;

function RootComponent() {
  return (
    <>
      <Outlet />
      {import.meta.env.DEV ? (
        <Suspense fallback={null}>
          <TanStackDevtools />
        </Suspense>
      ) : null}
    </>
  );
}
