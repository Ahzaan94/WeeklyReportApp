import { useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import api from '../api/client'
import TaskTable from '../components/TaskTable'
import FlaggedListEditor from '../components/FlaggedListEditor'
import StatusBadge from '../components/StatusBadge'

const TASK_TYPES = ['Development', 'Testing', 'Meetings', 'Documentation', 'Other']

function emptyReport() {
  return {
    projectId: '',
    weekStartDate: '',
    weekEndDate: '',
    tasks: [],
    plannedNextWeek: '',
    blockers: [],
    achievements: [],
    hoursByType: TASK_TYPES.map(t => ({ taskType: t, hours: 0 })),
    notes: ''
  }
}

export default function ReportFormPage() {
  const { id } = useParams()
  const isEdit = !!id
  const navigate = useNavigate()

  const [projects, setProjects] = useState([])
  const [report, setReport] = useState(emptyReport())
  const [status, setStatus] = useState('Draft')
  const [latestComment, setLatestComment] = useState(null)
  const [loading, setLoading] = useState(isEdit)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    api.get('/projects').then(res => setProjects(res.data))
  }, [])

  useEffect(() => {
    if (!isEdit) return
    api.get(`/reports/${id}`).then(res => {
      const r = res.data
      setReport({
        projectId: r.projectId,
        weekStartDate: r.weekStartDate.slice(0, 10),
        weekEndDate: r.weekEndDate.slice(0, 10),
        tasks: r.tasks,
        plannedNextWeek: r.plannedNextWeek || '',
        blockers: r.blockers,
        achievements: r.achievements,
        hoursByType: TASK_TYPES.map(t => ({
          taskType: t,
          hours: r.hoursByType.find(h => h.taskType === t)?.hours || 0
        })),
        notes: r.notes || ''
      })
      setStatus(r.status)
      setLatestComment(r.latestReviewerComment)
      setLoading(false)
    })
  }, [id])

  const editable = !isEdit || status === 'Draft' || status === 'NeedsCorrection'

  const save = async (submitAfter) => {
    setError('')
    setSaving(true)
    try {
      const payload = { ...report, projectId: Number(report.projectId) }
      let reportId = id
      if (isEdit) {
        await api.put(`/reports/${id}`, payload)
      } else {
        const res = await api.post('/reports', payload)
        reportId = res.data.id
      }
      if (submitAfter) {
        await api.post(`/reports/${reportId}/submit`)
      }
      navigate('/reports')
    } catch (err) {
      setError(JSON.stringify(err.response?.data) || 'Failed to save report.')
    } finally {
      setSaving(false)
    }
  }

  if (loading) return <div className="page-container">Loading...</div>

  return (
    <div className="page-container">
      <div className="page-header">
        <h1>{isEdit ? 'Edit Weekly Report' : 'New Weekly Report'}</h1>
        {isEdit && <StatusBadge status={status} />}
      </div>

      {latestComment && status === 'NeedsCorrection' && (
        <div className="correction-banner">
          <strong>Manager's feedback:</strong> {latestComment}
        </div>
      )}

      {error && <div className="error-banner">{error}</div>}
      {!editable && <div className="info-banner">This report is {status} and can no longer be edited.</div>}

      <div className="form-card">
        <div className="form-row">
          <div>
            <label>Project / Category</label>
            <select disabled={!editable} value={report.projectId} onChange={e => setReport({ ...report, projectId: e.target.value })} required>
              <option value="">Select a project...</option>
              {projects.map(p => <option key={p.id} value={p.id}>{p.name}</option>)}
            </select>
          </div>
          <div>
            <label>Week Start</label>
            <input type="date" disabled={!editable} value={report.weekStartDate} onChange={e => setReport({ ...report, weekStartDate: e.target.value })} required />
          </div>
          <div>
            <label>Week End</label>
            <input type="date" disabled={!editable} value={report.weekEndDate} onChange={e => setReport({ ...report, weekEndDate: e.target.value })} required />
          </div>
        </div>

        <h3>Tasks Completed</h3>
        <TaskTable tasks={report.tasks} onChange={tasks => setReport({ ...report, tasks })} readOnly={!editable} />

        <h3>Tasks Planned for Next Week</h3>
        <textarea disabled={!editable} rows={3} value={report.plannedNextWeek} onChange={e => setReport({ ...report, plannedNextWeek: e.target.value })} />

        <h3>Blockers / Challenges</h3>
        <FlaggedListEditor
          items={report.blockers}
          onChange={blockers => setReport({ ...report, blockers })}
          flagField="isKeyIssue"
          flagLabel="Key Issue"
          placeholder="Describe a blocker..."
          readOnly={!editable}
        />

        <h3>Achievements / Highlights</h3>
        <FlaggedListEditor
          items={report.achievements}
          onChange={achievements => setReport({ ...report, achievements })}
          flagField="isKeyAchievement"
          flagLabel="Key Achievement"
          placeholder="Describe an achievement..."
          readOnly={!editable}
        />

        <h3>Hours by Task Type <span className="hint-text">(optional)</span></h3>
        <div className="hours-grid">
          {report.hoursByType.map((h, i) => (
            <div key={h.taskType}>
              <label>{h.taskType}</label>
              <input
                type="number" min="0" step="0.5" disabled={!editable}
                value={h.hours}
                onChange={e => {
                  const next = [...report.hoursByType]
                  next[i] = { ...next[i], hours: Number(e.target.value) }
                  setReport({ ...report, hoursByType: next })
                }}
              />
            </div>
          ))}
        </div>

        <h3>Notes / Links <span className="hint-text">(optional)</span></h3>
        <textarea disabled={!editable} rows={2} value={report.notes} onChange={e => setReport({ ...report, notes: e.target.value })} />

        {editable && (
          <div className="form-actions">
            <button className="btn-secondary" onClick={() => save(false)} disabled={saving}>Save Draft</button>
            <button className="btn-primary" onClick={() => save(true)} disabled={saving}>Submit for Review</button>
          </div>
        )}
      </div>
    </div>
  )
}
