import { create } from "zustand";
import { persist } from "zustand/middleware";
import apiClient from "@/lib/api-client";

interface UserProfile {
  email: string;
  firstName: string;
  lastName: string;
  permissions: string[];
  role: string;
}

interface AuthState {
  clearAuth: () => void;
  fetchMe: () => Promise<void>;
  isAuthenticated: boolean;
  isInitialized: boolean;
  setAuth: (user: UserProfile) => void;
  user: UserProfile | null;
  wasLoggedIn: boolean;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      clearAuth: () => {
        set({
          isAuthenticated: false,
          isInitialized: true,
          user: null,
          wasLoggedIn: false,
        });
      },

      fetchMe: async () => {
        try {
          const response = await apiClient.get<UserProfile>("/v1/identity/me");
          set({
            isAuthenticated: true,
            isInitialized: true,
            user: response.data,
            wasLoggedIn: true,
          });
        } catch {
          set({
            isAuthenticated: false,
            isInitialized: true,
            user: null,
            wasLoggedIn: false,
          });
        }
      },
      isAuthenticated: false,
      isInitialized: false,

      setAuth: (user) => {
        set({
          isAuthenticated: true,
          isInitialized: true,
          user,
          wasLoggedIn: true,
        });
      },
      user: null,
      wasLoggedIn: false,
    }),
    {
      name: "auth_session",
      partialize: (state) => ({ wasLoggedIn: state.wasLoggedIn }),
    }
  )
);
