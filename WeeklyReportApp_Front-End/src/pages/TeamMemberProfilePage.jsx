import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import api from '../api/client'
import StatusBadge from '../components/StatusBadge'
import { LineChart, Line, XAxis, YAxis, Tooltip, ResponsiveContainer, CartesianGrid } from 'recharts'

export default function TeamMemberProfilePage() {
  const { userId } = useParams()
  const [member, setMember] = useState(null)
  const [reports, setReports] = useState([])
  const [trend, setTrend] = useState([])

  useEffect(() => {
    api.get('/users').then(res => setMember(res.data.find(u => u.id === userId)))
    api.get('/reports', { params: { userId, pageSize: 100 } }).then(res => setReports(res.data.items))
    api.get('/dashboard/charts/tasks-trend', { params: { userId } }).then(res => setTrend(res.data))
  }, [userId])

  const approvedCount = reports.filter(r => r.status === 'Approved').length
  const needsCorrectionCount = reports.filter(r => r.status === 'NeedsCorrection').length

  return (
    <div className="page-container">
      <h1>{member?.fullName || 'Team Member'}</h1>
      <p className="hint-text">{member?.email}</p>

      <div className="stats-row">
        <div className="stat-card"><span className="stat-value">{reports.length}</span><span>Total Reports</span></div>
        <div className="stat-card"><span className="stat-value">{approvedCount}</span><span>Approved</span></div>
        <div className="stat-card"><span className="stat-value">{needsCorrectionCount}</span><span>Needs Correction</span></div>
      </div>

      <div className="form-card">
        <h3>Tasks Completed Trend</h3>
        <ResponsiveContainer width="100%" height={250}>
          <LineChart data={trend}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis dataKey="weekLabel" />
            <YAxis allowDecimals={false} />
            <Tooltip />
            <Line type="monotone" dataKey="tasksCompleted" stroke="#2f7fd6" strokeWidth={2} />
          </LineChart>
        </ResponsiveContainer>
      </div>

      <div className="form-card">
        <h3>Report History</h3>
        <table className="data-table">
          <thead><tr><th>Week</th><th>Project</th><th>Status</th><th></th></tr></thead>
          <tbody>
            {reports.map(r => (
              <tr key={r.id}>
                <td>{new Date(r.weekStartDate).toLocaleDateString()}</td>
                <td>{r.projectName}</td>
                <td><StatusBadge status={r.status} /></td>
                <td>
                  {r.status === 'Submitted'
                    ? <Link to={`/team-reports/${r.id}/review`}>Review</Link>
                    : <Link to={`/reports/${r.id}`}>View</Link>}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}
