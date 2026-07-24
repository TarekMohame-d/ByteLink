# ============================
# Stage 1 - Build React App
# ============================
FROM node:24-bookworm-slim AS build

WORKDIR /src

# Enable pnpm via Corepack
ENV PNPM_HOME="/pnpm"
ENV PATH="$PNPM_HOME:$PATH"
RUN corepack enable

# Copy dependency files first for layer caching
COPY src/client/app/package.json .
COPY src/client/app/pnpm-lock.yaml .

# Install dependencies using BuildKit cache
RUN --mount=type=cache,id=pnpm,target=/pnpm/store \
    pnpm config set store-dir /pnpm/store && \
    pnpm install --frozen-lockfile

# Copy application source
COPY src/client/app .

# Declare build argument
ARG VITE_APP_BASE_URL
ENV VITE_APP_BASE_URL=$VITE_APP_BASE_URL

# Build client bundle
RUN pnpm build

# ============================
# Stage 2 - Production Runtime
# ============================
FROM nginxinc/nginx-unprivileged:1.27-alpine AS final

# Configure container health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/ || exit 1

# Copy custom Nginx configuration (configured to listen on 8080)
COPY docker/frontend/nginx.conf /etc/nginx/conf.d/default.conf

# Copy compiled static assets
COPY --from=build /src/dist /usr/share/nginx/html

EXPOSE 8080

CMD ["nginx", "-g", "daemon off;"]
