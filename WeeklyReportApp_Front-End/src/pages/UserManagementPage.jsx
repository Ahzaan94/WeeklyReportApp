import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/client'

export default function UserManagementPage() {
  const [users, setUsers] = useState([])

  const load = () => api.get('/users').then(res => setUsers(res.data))
  useEffect(() => { load() }, [])

  const changeRole = async (id, role) => {
    await api.put(`/users/${id}/role`, { role })
    load()
  }

  const toggleActive = async (id, isActive) => {
    await api.put(`/users/${id}/active`, { isActive: !isActive })
    load()
  }

  return (
    <div className="page-container">
      <h1>User Management</h1>
      <p className="hint-text">New users register themselves at the /register page. Managers can adjust roles and access here.</p>

      <table className="data-table">
        <thead><tr><th>Name</th><th>Email</th><th>Role</th><th>Status</th><th></th></tr></thead>
        <tbody>
          {users.map(u => (
            <tr key={u.id}>
              <td>{u.role === 'TeamMember' ? <Link to={`/team-members/${u.id}`}>{u.fullName}</Link> : u.fullName}</td>
              <td>{u.email}</td>
              <td>
                <select value={u.role} onChange={e => changeRole(u.id, e.target.value)}>
                  <option value="TeamMember">Team Member</option>
                  <option value="Manager">Manager</option>
                </select>
              </td>
              <td>{u.isActive ? 'Active' : 'Inactive'}</td>
              <td><button className="btn-link" onClick={() => toggleActive(u.id, u.isActive)}>{u.isActive ? 'Deactivate' : 'Activate'}</button></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
