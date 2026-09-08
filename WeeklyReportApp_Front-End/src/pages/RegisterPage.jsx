import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export default function RegisterPage() {
  const [fullName, setFullName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [role, setRole] = useState('TeamMember')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const { register } = useAuth()
  const navigate = useNavigate()

  const submit = async (e) => {
    e.preventDefault()
    setError('')
    setLoading(true)
    try {
      const data = await register(fullName, email, password, role)
      navigate(data.role === 'Manager' ? '/dashboard' : '/reports')
    } catch (err) {
      setError(JSON.stringify(err.response?.data) || 'Registration failed.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="auth-page">
      <form className="auth-card" onSubmit={submit}>
        <h1>Create Account</h1>
        {error && <div className="error-banner">{String(error)}</div>}
        <label>Full Name</label>
        <input value={fullName} onChange={e => setFullName(e.target.value)} required />
        <label>Email</label>
        <input type="email" value={email} onChange={e => setEmail(e.target.value)} required />
        <label>Password</label>
        <input type="password" value={password} onChange={e => setPassword(e.target.value)} required minLength={6} />
        <label>Role</label>
        <select value={role} onChange={e => setRole(e.target.value)}>
          <option value="TeamMember">Team Member</option>
          <option value="Manager">Manager</option>
        </select>
        <button className="btn-primary" type="submit" disabled={loading}>
          {loading ? 'Creating...' : 'Register'}
        </button>
        <p className="auth-footer">Already have an account? <Link to="/login">Login</Link></p>
      </form>
    </div>
  )
}
