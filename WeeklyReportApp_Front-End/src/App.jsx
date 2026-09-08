import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider, useAuth } from './context/AuthContext'
import ProtectedRoute from './components/ProtectedRoute'
import Navbar from './components/Navbar'

import LoginPage from './pages/LoginPage'
import RegisterPage from './pages/RegisterPage'
import ReportFormPage from './pages/ReportFormPage'
import ReportHistoryPage from './pages/ReportHistoryPage'
import ReportDetailPage from './pages/ReportDetailPage'
import TeamReportsPage from './pages/TeamReportsPage'
import ManagerReviewPage from './pages/ManagerReviewPage'
import TeamMemberProfilePage from './pages/TeamMemberProfilePage'
import ProjectsPage from './pages/ProjectsPage'
import UserManagementPage from './pages/UserManagementPage'
import DashboardPage from './pages/DashboardPage'

function HomeRedirect() {
  const { user } = useAuth()
  if (!user) return <Navigate to="/login" replace />
  return <Navigate to={user.role === 'Manager' ? '/dashboard' : '/reports'} replace />
}

function AppRoutes() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/" element={<HomeRedirect />} />

        {/* Team member routes */}
        <Route path="/reports" element={<ProtectedRoute><ReportHistoryPage /></ProtectedRoute>} />
        <Route path="/reports/new" element={<ProtectedRoute><ReportFormPage /></ProtectedRoute>} />
        <Route path="/reports/:id" element={<ProtectedRoute><ReportDetailPage /></ProtectedRoute>} />
        <Route path="/reports/:id/edit" element={<ProtectedRoute><ReportFormPage /></ProtectedRoute>} />

        {/* Manager routes */}
        <Route path="/dashboard" element={<ProtectedRoute managerOnly><DashboardPage /></ProtectedRoute>} />
        <Route path="/team-reports" element={<ProtectedRoute managerOnly><TeamReportsPage /></ProtectedRoute>} />
        <Route path="/team-reports/:id/review" element={<ProtectedRoute managerOnly><ManagerReviewPage /></ProtectedRoute>} />
        <Route path="/team-members/:userId" element={<ProtectedRoute managerOnly><TeamMemberProfilePage /></ProtectedRoute>} />
        <Route path="/projects" element={<ProtectedRoute managerOnly><ProjectsPage /></ProtectedRoute>} />
        <Route path="/users" element={<ProtectedRoute managerOnly><UserManagementPage /></ProtectedRoute>} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  )
}

export default function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  )
}
