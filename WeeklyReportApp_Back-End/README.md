# Backend Setup — Weekly Report App API (ASP.NET Core + MSSQL)

## Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB, SQL Express, full SQL Server, or Docker `mcr.microsoft.com/mssql/server`)
- EF Core CLI tools: `dotnet tool install --global dotnet-ef`

## 1. Configure the connection string
Edit `WeeklyReportApp.API/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=WeeklyReportAppDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
}
```
Also replace `Jwt:Key` with your own long random secret (32+ characters).

## 2. Install dependencies
```bash
cd WeeklyReportApp.API
dotnet restore
```

## 3. Create the database (EF Core Migrations)
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
> The app also calls `db.Database.Migrate()` automatically on startup, so once migrations exist they'll apply automatically each run.

## 4. Run the backend
```bash
dotnet run
```
API will be available at `https://localhost:5001` (Swagger UI at `/swagger`).

## 5. Demo data
On first run, the app seeds:
- 1 manager (`manager@demo.com` / `Passw0rd!`)
- 5 team members (`priya@demo.com`, `daniel@demo.com`, `maria@demo.com`, `john@demo.com`, `wei@demo.com`, all password `Passw0rd!`)
- 4 projects
- 4 weeks of reports across all members in Draft / Submitted / Needs Correction / Approved states

## Project structure
```
WeeklyReportApp.API/
  Controllers/     -> AuthController, ReportsController, ProjectsController, DashboardController
  Models/          -> EF Core entities (ApplicationUser, WeeklyReport, ReportVersion, etc.)
  DTOs/            -> Request/response shapes
  Data/            -> AppDbContext, DbInitializer (seeding)
  Services/        -> TokenService (JWT)
```

## Key design notes
- **Auth**: ASP.NET Identity + JWT bearer tokens. Roles: `TeamMember`, `Manager`.
- **RBAC**: enforced via `[Authorize(Roles = ...)]` on controllers/actions, plus explicit ownership checks
  (`report.UserId != CurrentUserId → Forbid()`) inside `ReportsController` so a team member can never
  read/edit another member's report.
- **Review workflow**: `Draft → Submitted → NeedsCorrection → Approved`, enforced in `ReportsController.Submit`
  and `.Review`. Managers can only hit the `/review` endpoint, which touches status + comment fields only —
  the report content fields are never writable by a manager.
- **Version history**: `ReportVersion` snapshots are written automatically whenever an edit is made to a
  report that's in `NeedsCorrection` status, before the new content overwrites it. `GET /api/reports/{id}/versions`
  lists all snapshots; `GET /api/reports/{id}/versions/{versionNumber}` returns one.
- **Pagination/filtering**: `GET /api/reports` (manager) and `/api/reports/mine` (team member) both support
  `page`, `pageSize`, and filters (`userId`, `projectId`, `fromDate`, `toDate`, `status`).
