import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function Navbar() {
  const { user, logout, isManager } = useAuth()
  const navigate = useNavigate()

  if (!user) return null

  const doLogout = () => {
    logout()
    navigate('/login')
  }

  return (
    <nav className="navbar">
      <div className="navbar-brand">Weekly Report App</div>
      <div className="navbar-links">
        {!isManager && (
          <>
            <Link to="/reports/new">New Report</Link>
            <Link to="/reports">My Reports</Link>
          </>
        )}
        {isManager && (
          <>
            <Link to="/dashboard">Dashboard</Link>
            <Link to="/team-reports">Team Reports</Link>
            <Link to="/projects">Projects</Link>
            <Link to="/users">Users</Link>
          </>
        )}
      </div>
      <div className="navbar-user">
        <span>{user.fullName} ({user.role})</span>
        <button onClick={doLogout}>Logout</button>
      </div>
    </nav>
  )
}
