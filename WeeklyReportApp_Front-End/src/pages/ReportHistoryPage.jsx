import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/client'
import StatusBadge from '../components/StatusBadge'

export default function ReportHistoryPage() {
  const [reports, setReports] = useState([])
  const [statusFilter, setStatusFilter] = useState('')
  const [loading, setLoading] = useState(true)

  const load = () => {
    setLoading(true)
    api.get('/reports/mine', { params: { status: statusFilter || undefined, pageSize: 50 } })
      .then(res => setReports(res.data.items))
      .finally(() => setLoading(false))
  }

  useEffect(load, [statusFilter])

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>My Reports</h1>
        <Link className="btn-primary" to="/reports/new">+ New Report</Link>
      </div>

      <div className="filter-bar">
        <label>Status:</label>
        <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)}>
          <option value="">All</option>
          <option value="Draft">Draft</option>
          <option value="Submitted">Submitted</option>
          <option value="NeedsCorrection">Needs Correction</option>
          <option value="Approved">Approved</option>
        </select>
      </div>

      {loading ? <p>Loading...</p> : (
        <table className="data-table">
          <thead>
            <tr><th>Week</th><th>Project</th><th>Status</th><th>Last Updated</th><th></th></tr>
          </thead>
          <tbody>
            {reports.map(r => (
              <tr key={r.id}>
                <td>{new Date(r.weekStartDate).toLocaleDateString()} - {new Date(r.weekEndDate).toLocaleDateString()}</td>
                <td>{r.projectName}</td>
                <td><StatusBadge status={r.status} /></td>
                <td>{new Date(r.updatedAt).toLocaleString()}</td>
                <td>
                  <Link to={`/reports/${r.id}`}>View</Link>
                  {(r.status === 'Draft' || r.status === 'NeedsCorrection') && (
                    <> · <Link to={`/reports/${r.id}/edit`}>Edit</Link></>
                  )}
                </td>
              </tr>
            ))}
            {reports.length === 0 && <tr><td colSpan={5}>No reports yet.</td></tr>}
          </tbody>
        </table>
      )}
    </div>
  )
}
