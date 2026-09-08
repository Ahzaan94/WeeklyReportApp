import { useEffect, useState } from 'react'
import api from '../api/client'
import {
  LineChart, Line, BarChart, Bar, PieChart, Pie, Cell,
  XAxis, YAxis, Tooltip, Legend, CartesianGrid, ResponsiveContainer
} from 'recharts'

const STATUS_COLORS = { Draft: '#8b8b8b', Submitted: '#2f7fd6', NeedsCorrection: '#d9822b', Approved: '#2fa84f', NotStarted: '#c2c2c2' }
const PIE_COLORS = ['#2f7fd6', '#2fa84f', '#d9822b', '#a35bd9', '#e0518a']

function getMonday(date) {
  const d = new Date(date)
  const day = d.getDay()
  const diff = d.getDate() - day + (day === 0 ? -6 : 1)
  return new Date(d.setDate(diff)).toISOString().slice(0, 10)
}

export default function DashboardPage() {
  const [weekStart, setWeekStart] = useState(getMonday(new Date()))
  const [summary, setSummary] = useState(null)
  const [memberStatus, setMemberStatus] = useState([])
  const [tasksTrend, setTasksTrend] = useState([])
  const [workload, setWorkload] = useState([])
  const [timeByType, setTimeByType] = useState([])
  const [activity, setActivity] = useState([])
  const [section, setSection] = useState('blockers')
  const [sectionData, setSectionData] = useState([])

  useEffect(() => {
    api.get('/dashboard/summary', { params: { weekStartDate: weekStart } }).then(res => setSummary(res.data))
    api.get('/dashboard/member-status', { params: { weekStartDate: weekStart } }).then(res => setMemberStatus(res.data))
    api.get('/dashboard/charts/tasks-trend').then(res => setTasksTrend(res.data))
    api.get('/dashboard/charts/workload-by-project').then(res => setWorkload(res.data))
    api.get('/dashboard/charts/time-by-task-type').then(res => setTimeByType(res.data))
    api.get('/dashboard/activity').then(res => setActivity(res.data))
  }, [weekStart])

  useEffect(() => {
    api.get(`/dashboard/section/${section}`, { params: { weekStartDate: weekStart } }).then(res => setSectionData(res.data))
  }, [section, weekStart])

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>Team Dashboard</h1>
        <div>
          <label>Week of </label>
          <input type="date" value={weekStart} onChange={e => setWeekStart(e.target.value)} />
        </div>
      </div>

      {summary && (
        <div className="stats-row">
          <div className="stat-card"><span className="stat-value">{summary.totalReportsThisWeek}</span><span>Reports This Week</span></div>
          <div className="stat-card"><span className="stat-value">{summary.submittedCount}</span><span>Submitted/Approved</span></div>
          <div className="stat-card"><span className="stat-value">{summary.pendingCount}</span><span>Pending</span></div>
          <div className="stat-card"><span className="stat-value">{summary.lateCount}</span><span>Late</span></div>
          <div className="stat-card"><span className="stat-value">{summary.needsCorrectionCount}</span><span>Needs Correction</span></div>
          <div className="stat-card"><span className="stat-value">{summary.openBlockersCount}</span><span>Open Blockers</span></div>
        </div>
      )}

      <div className="charts-grid">
        <div className="form-card">
          <h3>Tasks Completed Trend</h3>
          <ResponsiveContainer width="100%" height={220}>
            <LineChart data={tasksTrend}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="weekLabel" /><YAxis allowDecimals={false} /><Tooltip />
              <Line type="monotone" dataKey="tasksCompleted" stroke="#2f7fd6" strokeWidth={2} />
            </LineChart>
          </ResponsiveContainer>
        </div>

        <div className="form-card">
          <h3>Status by Team Member (this week)</h3>
          <table className="data-table compact">
            <tbody>
              {memberStatus.map(m => (
                <tr key={m.userId}>
                  <td>{m.userFullName}</td>
                  <td>
                    <span className="status-dot" style={{ backgroundColor: STATUS_COLORS[m.status] }} /> {m.status}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="form-card">
          <h3>Workload by Project (hours)</h3>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={workload}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="projectName" /><YAxis /><Tooltip />
              <Bar dataKey="totalHours" fill="#2f7fd6" />
            </BarChart>
          </ResponsiveContainer>
        </div>

        <div className="form-card">
          <h3>Time by Task Type (team-wide)</h3>
          <ResponsiveContainer width="100%" height={220}>
            <PieChart>
              <Pie data={timeByType} dataKey="totalHours" nameKey="taskType" outerRadius={80} label>
                {timeByType.map((_, i) => <Cell key={i} fill={PIE_COLORS[i % PIE_COLORS.length]} />)}
              </Pie>
              <Tooltip /><Legend />
            </PieChart>
          </ResponsiveContainer>
        </div>
      </div>

      <div className="form-card">
        <div className="page-header">
          <h3>Compare a Section Across the Team</h3>
          <select value={section} onChange={e => setSection(e.target.value)}>
            <option value="blockers">Blockers</option>
            <option value="achievements">Achievements</option>
          </select>
        </div>
        <table className="data-table">
          <thead><tr><th>Member</th><th>{section === 'blockers' ? 'Blockers' : 'Achievements'}</th></tr></thead>
          <tbody>
            {sectionData.map((s, i) => (
              <tr key={i}>
                <td>{s.userFullName}</td>
                <td>{s.items.length ? s.items.join('; ') : '—'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="form-card">
        <h3>Recent Activity</h3>
        <ul className="activity-feed">
          {activity.map((a, i) => (
            <li key={i}>{a.description} — <span className="hint-text">{a.actorName}, {new Date(a.timestamp).toLocaleString()}</span></li>
          ))}
          {activity.length === 0 && <li>No recent activity.</li>}
        </ul>
      </div>
    </div>
  )
}
