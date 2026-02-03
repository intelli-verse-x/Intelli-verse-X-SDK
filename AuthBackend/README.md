# AuthBackend — ASP.NET Core 7 Web API

Authentication backend for the Unity client: email OTP registration, OTP verification login, Google/Apple/Guest login, JWT access + refresh tokens.

## Run

```bash
cd AuthBackend
dotnet restore
dotnet run
```

API: **http://localhost:3000**  
Swagger: **http://localhost:3000/swagger**

## Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | `/auth/register` | Generate OTP, send via email |
| POST | `/auth/verify-otp` | Verify OTP, issue JWT tokens |
| POST | `/auth/google` | Verify Google ID token, issue JWT tokens |
| POST | `/auth/apple` | Verify Apple identity token, issue JWT tokens |
| POST | `/auth/guest` | Create guest user, issue JWT tokens |

## Configuration

Edit **appsettings.json** (or use environment variables / User Secrets):

- **Jwt:SecretKey** — Min 32 characters; used to sign JWTs (7-day expiry).
- **Smtp** — Gmail: `smtp.gmail.com`, port `587`, SSL `true`. Use App Password for `Password`.
- **Google:ClientId** — Your Google OAuth client ID (optional; if empty, Google token validation skips audience check).
- **Apple:ClientId** — Your Apple Services ID / bundle ID for Apple Sign In.

## Unity

Point the Unity `AuthService` base URL to `http://localhost:3000/auth` (or your deployed URL). Ensure the Unity project’s auth flow uses the same endpoints and request/response shapes as this API.
