# ByteLink 🔗

ByteLink is a high-performance URL shortening service built with **ASP.NET Core 10** and **React**. It follows the **Backend-for-Frontend (BFF)** pattern for secure authentication and uses **Redis** for low-latency redirection.

## 🚀 Features

- **Blazing Fast Redirection:** < 50ms redirection using Redis caching.
- **Secure by Design:** HttpOnly cookie-based authentication via Keycloak (OIDC).
- **Smart URL Normalization:** Prevents duplicate codes by stripping tracking parameters (UTM).
- **Favicon Fetching:** Automatically fetches and caches favicons for shortened URLs.
- **Rate Limiting:** Built-in protection against abuse for both creation and redirection.
- **Modern Tech Stack:** .NET 10, PostgreSQL, Redis, React, and TypeScript.
- **BFF Pattern:** Automated token refresh and secure session management on the server.

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core 10
- **Database:** PostgreSQL (EF Core)
- **Caching:** Redis
- **Identity:** Keycloak (OpenID Connect)
- **Frontend:** React + TypeScript + Vite
- **API Docs:** Scalar / OpenAPI 3.1

## 🏗️ Architecture

ByteLink uses a **Feature-based architecture** in the backend, grouping logic by business capabilities rather than technical layers. It utilizes the **Mediator pattern** (via custom `IRequestHandler`) and **Carter** for clean, minimal API routing.

### Authentication Flow (BFF)
1. User clicks login -> Redirects to `/api/login`.
2. Backend initiates OIDC flow with Keycloak.
3. On success, backend sets a secure, HttpOnly `BFF-Session` cookie.
4. Backend handles token storage and automatic refresh transparently.

## 🚦 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker & Docker Compose](https://www.docker.com/)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/your-username/ByteLink.git
   cd ByteLink
   ```

2. **Spin up Infrastructure** (PostgreSQL, Redis, Keycloak)
   ```bash
   docker-compose up -d
   ```

3. **Backend Setup**
   - Navigate to `src/ByteLink.Api`
   - Create a `.env` file (see `.env.example`)
   - Run migrations: `dotnet ef database update`
   - Start the API: `dotnet run`

4. **Frontend Setup**
   - Navigate to `src/client`
   - Install dependencies: `pnpm install`
   - Start development server: `pnpm dev`

## 📖 API Documentation

Once the backend is running, access the interactive API documentation at:
- **Scalar:** `http://localhost:5000/scalar/v1`
- **OpenAPI JSON:** `http://localhost:5000/openapi/v1.json`

## 🧪 Testing

```bash
dotnet test
```

## 📄 License

MIT
