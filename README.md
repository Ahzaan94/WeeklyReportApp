# Weekly Report Generator & Team Dashboard

A full-stack web app for submitting weekly work reports, reviewing them through an approval workflow, and tracking team activity on a manager dashboard.

**Stack:** React (Vite) · ASP.NET Core · MSSQL

## Features

- Team Member and Manager roles with JWT authentication
- Weekly report submission with a fixed, consistent structure
- Review workflow: Draft → Submitted → Needs Correction → Approved
- Report version history (previous versions kept when a report is corrected)
- Manager dashboard with summary metrics and charts
- Project/category management
- User management (roles, active status)

## Project Structure

```
backend/    ASP.NET Core Web API + EF Core (MSSQL)
frontend/   React (Vite) single-page app
```

Each folder has its own README with more detail. Quick start below.

## Prerequisites

- .NET 8 SDK
- Node.js 18+
- SQL Server (LocalDB, SQL Express, full SQL Server, or Docker)

## 1. Install Dependencies

```bash
# WeeklyReportApp_Back-End
cd WeeklyReportApp_Back-End/WeeklyReportApp.API
dotnet restore

# WeeklyReportApp_Front-End
cd WeeklyReportApp_Front-End
npm install
```

## 2. Run the Database

Update the connection string in `WeeklyReportApp_Back-End/WeeklyReportApp.API/appsettings.json` to point at your SQL Server instance, then create the database:

```bash
cd WeeklyReportApp_Back-End/WeeklyReportApp.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## 3. Run the Backend

```bash
cd WeeklyReportApp_Back-End/WeeklyReportApp.API
dotnet run
```
API runs at `https://localhost:52757` (Swagger at `/swagger`). Demo data (manager, team members, projects, sample reports) is seeded automatically on first run.

## 4. Run the Frontend

```bash
cd WeeklyReportApp_Front-End
cp .env.example .env
npm run dev
```
App runs at `http://localhost:5173`.

## Demo Logins

| Role | Email | Password |
|------|-------|----------|
| Manager | manager@demo.com | Passw0rd! |
| Team Member | priya@demo.com | Passw0rd! |

(Other seeded team members: daniel@demo.com, maria@demo.com, john@demo.com, wei@demo.com — same password.)
