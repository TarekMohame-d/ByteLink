import axios, { type AxiosRequestConfig, type AxiosResponse } from "axios";
import { env } from "@/config/env";
import { useAuthStore } from "@/store/authStore";
import type { ApiError } from "@/types/api";
import { failure, type Result, success } from "@/types/result";
import { getOrCreateDeviceIdentity } from "@/utils/device-identity";

interface QueueItem {
  reject: (error: unknown) => void;
  resolve: () => void;
}

interface CustomAxiosRequestConfig extends AxiosRequestConfig {
  _retry?: boolean;
}

const identity = getOrCreateDeviceIdentity();

const apiClient = axios.create({
  baseURL: env.BASE_URL,
  headers: {
    Accept: "application/json",
    "Content-Type": "application/json",
    "X-Device-Id": identity.deviceId,
    "X-Device-Metadata": identity.deviceMetadata,
  },
  timeout: 10_000,
  withCredentials: true,
});

let isRefreshing = false;
let failedQueue: QueueItem[] = [];

const processQueue = (error: unknown) => {
  for (const prom of failedQueue) {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve();
    }
  }
  failedQueue = [];
};

apiClient.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) {
      return Promise.reject(error);
    }

    const originalRequest = error.config as
      | CustomAxiosRequestConfig
      | undefined;

    if (!originalRequest) {
      return Promise.reject(error);
    }

    const requestUrl = originalRequest.url ?? "";

    if (requestUrl.includes("/v1/identity/refresh-token")) {
      useAuthStore.getState().clearAuth();
      if (window.location.pathname !== "/auth/sign-in") {
        window.location.href = "/auth/sign-in";
      }
      return Promise.reject(error);
    }

    if (requestUrl.includes("/v1/identity/login")) {
      return Promise.reject(error);
    }

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise<void>((resolve, reject) => {
          failedQueue.push({ reject, resolve });
        })
          .then(() => apiClient(originalRequest))
          .catch((err: unknown) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        await apiClient.post("/v1/identity/refresh-token");

        isRefreshing = false;
        processQueue(null);

        return apiClient(originalRequest);
      } catch (refreshError: unknown) {
        isRefreshing = false;
        processQueue(refreshError);

        useAuthStore.getState().clearAuth();
        if (window.location.pathname !== "/auth/sign-in") {
          window.location.href = "/auth/sign-in";
        }
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export function parseError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data;
    const status = error.response?.status;

    const message =
      data?.detail ||
      data?.errorDescription ||
      data?.title ||
      error.message ||
      "An unexpected error occurred.";

    return {
      detail: data?.detail,
      errorCode: data?.errorCode,
      errorDescription: data?.errorDescription,
      errors: data?.errors,
      message,
      status,
      title: data?.title,
      traceId: data?.traceId,
    };
  }

  return {
    message:
      error instanceof Error
        ? error.message
        : "An unknown network error occurred.",
  };
}

export async function safeRequest<T>(
  config: AxiosRequestConfig
): Promise<Result<T, ApiError>> {
  try {
    const response: AxiosResponse<T> = await apiClient(config);
    return success(response.data);
  } catch (error) {
    return failure(parseError(error));
  }
}

export default apiClient;
