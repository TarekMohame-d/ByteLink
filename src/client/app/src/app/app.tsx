import { QueryClient } from "@tanstack/react-query";
import { createRouter, RouterProvider } from "@tanstack/react-router";
import { routeTree } from "@/routeTree.gen";
import { useAuthStore } from "@/store/authStore";

export const queryClient = new QueryClient();

const router = createRouter({
  context: {
    auth: useAuthStore,
    queryClient,
  },
  defaultNotFoundComponent: () => <div>Not Found</div>,
  defaultPreload: "intent",
  routeTree,
});

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}

function App() {
  return <RouterProvider router={router} />;
}

export default App;
