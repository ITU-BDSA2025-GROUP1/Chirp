
- https://bdsagroup1chirprazor-guaycyegcagyewgd.norwayeast-01.azurewebsites.net

## Project overview
Chirp! is a teaching/sample application that demonstrates:
- Razor Pages web UI with ASP.NET Core Identity
- Local persistence with SQLite
- External authentication via GitHub OAuth (optional)
- CI with GitHub Actions and deployment to Azure App Service
- Automated EF Core migrations + basic DB seeding for demo data

## Getting started — prerequisites
- .NET 8 SDK
- dotnet-ef (for migrations): dotnet tool install --global dotnet-ef
- Optional: GitHub account and GitHub OAuth app for external login
- (Windows/macOS/Linux) A terminal and a code editor (VS Code recommended)

## Local development (recommended)
1. Clone the repo:
   git clone https://github.com/ITU-BDSA2025-GROUP1/Chirp.git
   cd Chirp

2. Restore and build:
   dotnet restore
   dotnet build

3. Configure secrets (local dev)
   - Use dotnet user-secrets (run from the Chirp.Web project folder):
     dotnet user-secrets init --project src/Chirp.Web/Chirp.Web.csproj
     dotnet user-secrets set "authentication:github:clientId" "<GITHUB_CLIENT_ID>"
     dotnet user-secrets set "authentication:github:clientSecret" "<GITHUB_CLIENT_SECRET>"
   - If you do not set the GitHub secrets, GitHub OAuth will be disabled (the app logs a warning) — safe for local dev.

4. Run the app (HTTPS recommended)
   - Trust dev cert:
     dotnet dev-certs https --trust 
   - Start the web app:
     dotnet run --project src/Chirp.Web/Chirp.Web.csproj
   - By default the app creates a SQLite DB file in:
     - Local dev: {repo-root}/src/Chirp.Web/App_Data/chirp.db
     - Azure App Service: D:\home\data\chirp.db (the app writes to HOME\data on App Service)
   - On first run the application will apply EF migrations and seed demo data automatically (unless running in the `Testing` environment).

5. Run tests:
   - From repo root:
     dotnet test

## Database and migrations
- This project uses SQLite in development and on App Service (no SQL Server required).
- Migrations live in the Chirp.Infrastructure project. Typical commands:
  - Add migration:
    dotnet ef migrations add MyMigration -p src/Chirp.Infrastructure -s src/Chirp.Web
  - Apply migrations locally (Program.cs already does this on startup), or:
    dotnet ef database update -p src/Chirp.Infrastructure -s src/Chirp.Web

## Seeding and test behavior
- The application seeds demo Authors and Cheeps on startup (except when the environment is `Testing`, because tests create and seed their own in-memory DB).
- If you remove the DB file, the app will recreate it and re-seed the demo data on next startup.

## License
This repository is currently using the MIT license by default. Update this section if your group uses a different license.