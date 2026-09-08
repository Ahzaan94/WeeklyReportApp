import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/client'
import StatusBadge from '../components/StatusBadge'

export default function TeamReportsPage() {
  const [reports, setReports] = useState([])
  const [members, setMembers] = useState([])
  const [projects, setProjects] = useState([])
  const [filters, setFilters] = useState({ userId: '', projectId: '', status: '', fromDate: '', toDate: '' })
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.get('/users').then(res => setMembers(res.data.filter(u => u.role === 'TeamMember')))
    api.get('/projects').then(res => setProjects(res.data))
  }, [])

  const load = () => {
    setLoading(true)
    const params = { pageSize: 100 }
    Object.entries(filters).forEach(([k, v]) => { if (v) params[k] = v })
    api.get('/reports', { params }).then(res => setReports(res.data.items)).finally(() => setLoading(false))
  }

  useEffect(load, [filters])

  return (
    <div className="page-container">
      <h1>Team Reports</h1>

      <div className="filter-bar">
        <select value={filters.userId} onChange={e => setFilters({ ...filters, userId: e.target.value })}>
          <option value="">All Members</option>
          {members.map(m => <option key={m.id} value={m.id}>{m.fullName}</option>)}
        </select>
        <select value={filters.projectId} onChange={e => setFilters({ ...filters, projectId: e.target.value })}>
          <option value="">All Projects</option>
          {projects.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
        </select>
        <select value={filters.status} onChange={e => setFilters({ ...filters, status: e.target.value })}>
          <option value="">All Statuses</option>
          <option value="Draft">Draft</option>
          <option value="Submitted">Submitted</option>
          <option value="NeedsCorrection">Needs Correction</option>
          <option value="Approved">Approved</option>
        </select>
        <label>From</label>
        <input type="date" value={filters.fromDate} onChange={e => setFilters({ ...filters, fromDate: e.target.value })} />
        <label>To</label>
        <input type="date" value={filters.toDate} onChange={e => setFilters({ ...filters, toDate: e.target.value })} />
      </div>

      {loading ? <p>Loading...</p> : (
        <table className="data-table">
          <thead>
            <tr><th>Member</th><th>Project</th><th>Week</th><th>Status</th><th>Updated</th><th></th></tr>
          </thead>
          <tbody>
            {reports.map(r => (
              <tr key={r.id}>
                <td><Link to={`/team-members/${r.userId}`}>{r.userFullName}</Link></td>
                <td>{r.projectName}</td>
                <td>{new Date(r.weekStartDate).toLocaleDateString()}</td>
                <td><StatusBadge status={r.status} /></td>
                <td>{new Date(r.updatedAt).toLocaleString()}</td>
                <td>
                  {r.status === 'Submitted'
                    ? <Link to={`/team-reports/${r.id}/review`}>Review</Link>
                    : <Link to={`/reports/${r.id}`}>View</Link>}
                </td>
              </tr>
            ))}
            {reports.length === 0 && <tr><td colSpan={6}>No reports match these filters.</td></tr>}
          </tbody>
        </table>
      )}
    </div>
  )
}
