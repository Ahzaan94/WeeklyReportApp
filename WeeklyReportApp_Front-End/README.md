# Frontend Setup — React (Vite)

## Prerequisites
- Node.js 18+

## 1. Install dependencies
```bash
cd frontend
npm install
```

## 2. Configure the API URL
Copy `.env.example` to `.env` and point it at your running backend:
```
VITE_API_BASE_URL=https://localhost:5001/api
```

## 3. Run the dev server
```bash
npm run dev
```
App runs at `http://localhost:5173`. Make sure the backend's CORS policy (`Program.cs`) includes this origin — it already does by default.

## Demo logins (seeded by the backend)
- Manager: `manager@demo.com` / `Passw0rd!`
- Team members: `priya@demo.com`, `daniel@demo.com`, `maria@demo.com`, `john@demo.com`, `wei@demo.com` (all `Passw0rd!`)

## Project structure
```
src/
  api/client.js          -> Axios instance with JWT interceptor
  context/AuthContext.jsx -> Auth state, login/register/logout
  components/            -> Navbar, ProtectedRoute, StatusBadge, TaskTable,
                             FlaggedListEditor (blockers/achievements), ReportView (shared read-only report)
  pages/
    LoginPage, RegisterPage
    ReportFormPage        -> Personal report create/edit (Section 2)
    ReportHistoryPage      -> Team member's own report list (Section 2 / 7)
    ReportDetailPage       -> Read-only report view, both roles (Section 7)
    TeamReportsPage        -> Manager's filterable team report list (Section 4)
    ManagerReviewPage      -> Approve / Request Changes action (Section 7)
    TeamMemberProfilePage  -> Per-member history + stats (Section 7)
    ProjectsPage           -> Project/category CRUD (Section 5 / 7)
    UserManagementPage     -> Role & active-status admin (Section 7)
    DashboardPage          -> Summary metrics + 4 charts + activity feed + cross-team section view (Section 6)
```

## Notes on how key requirements are implemented
- **Fixed report structure**: the report form (`ReportFormPage`) has a hardcoded set of fields/sections
  in a fixed order — team members cannot add, remove, or reorder sections.
- **Role-based UI**: `Navbar` and route guards (`ProtectedRoute`) hide manager-only pages from team members,
  matching the backend's role enforcement (this is a UX convenience — the backend is the real security boundary).
- **Review/correction cycle**: editing is only allowed while a report is `Draft` or `NeedsCorrection`
  (enforced both by disabling inputs client-side and by the backend rejecting other states).
- **Version history**: `ReportView` fetches `/reports/{id}/versions` and lets you open any past snapshot.
- **Charts**: built with Recharts, pulling live data from `/api/dashboard/charts/*`.
