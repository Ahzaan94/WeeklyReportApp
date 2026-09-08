import { useEffect, useState } from 'react'
import api from '../api/client'
import TaskTable from './TaskTable'
import FlaggedListEditor from './FlaggedListEditor'
import StatusBadge from './StatusBadge'

const TASK_TYPES = ['Development', 'Testing', 'Meetings', 'Documentation', 'Other']

export default function ReportView({ reportId, allowReview = false, onReviewed }) {
  const [report, setReport] = useState(null)
  const [versions, setVersions] = useState([])
  const [comments, setComments] = useState([])
  const [openVersion, setOpenVersion] = useState(null)
  const [comment, setComment] = useState('')
  const [reviewing, setReviewing] = useState(false)
  const [error, setError] = useState('')

  const load = () => {
    api.get(`/reports/${reportId}`).then(res => setReport(res.data))
    api.get(`/reports/${reportId}/versions`).then(res => setVersions(res.data))
    api.get(`/reports/${reportId}/comments`).then(res => setComments(res.data))
  }

  useEffect(load, [reportId])

  const doReview = async (approve) => {
    setError('')
    if (!approve && !comment.trim()) {
      setError('A comment is required when requesting changes.')
      return
    }
    setReviewing(true)
    try {
      await api.post(`/reports/${reportId}/review`, { approve, comment })
      setComment('')
      load()
      onReviewed?.()
    } catch (err) {
      setError(JSON.stringify(err.response?.data) || 'Review action failed.')
    } finally {
      setReviewing(false)
    }
  }

  const loadVersion = async (versionNumber) => {
    const res = await api.get(`/reports/${reportId}/versions/${versionNumber}`)
    setOpenVersion({ versionNumber, ...JSON.parse(res.data.contentSnapshotJson), submittedAt: res.data.submittedAt })
  }

  if (!report) return <p>Loading...</p>

  const hoursDisplay = TASK_TYPES.map(t => ({
    taskType: t,
    hours: report.hoursByType.find(h => h.taskType === t)?.hours || 0
  }))

  return (
    <div>
      <div className="page-header">
        <h1>{report.userFullName}'s Report</h1>
        <StatusBadge status={report.status} />
      </div>
      <p className="hint-text">
        {report.projectName} · Week of {new Date(report.weekStartDate).toLocaleDateString()} - {new Date(report.weekEndDate).toLocaleDateString()}
        {' · '}Version {report.currentVersionNumber}
      </p>

      {report.latestReviewerComment && (
        <div className="correction-banner"><strong>Latest reviewer comment:</strong> {report.latestReviewerComment}</div>
      )}

      <div className="form-card">
        <h3>Tasks Completed</h3>
        <TaskTable tasks={report.tasks} readOnly onChange={() => {}} />

        <h3>Tasks Planned for Next Week</h3>
        <p>{report.plannedNextWeek || '—'}</p>

        <h3>Blockers / Challenges</h3>
        <FlaggedListEditor items={report.blockers} flagField="isKeyIssue" flagLabel="Key Issue" readOnly onChange={() => {}} />

        <h3>Achievements / Highlights</h3>
        <FlaggedListEditor items={report.achievements} flagField="isKeyAchievement" flagLabel="Key Achievement" readOnly onChange={() => {}} />

        <h3>Hours by Task Type</h3>
        <div className="hours-grid">
          {hoursDisplay.map(h => (
            <div key={h.taskType}><label>{h.taskType}</label><span>{h.hours}h</span></div>
          ))}
        </div>

        <h3>Notes</h3>
        <p>{report.notes || '—'}</p>
      </div>

      {versions.length > 0 && (
        <div className="form-card">
          <h3>Previous Versions</h3>
          <ul className="version-list">
            {versions.map(v => (
              <li key={v.versionNumber}>
                Version {v.versionNumber} — submitted {new Date(v.submittedAt).toLocaleString()}{' '}
                <button className="btn-link" onClick={() => loadVersion(v.versionNumber)}>View</button>
              </li>
            ))}
          </ul>
          {openVersion && (
            <div className="version-preview">
              <h4>Version {openVersion.versionNumber} snapshot ({new Date(openVersion.submittedAt).toLocaleString()})</h4>
              <pre>{JSON.stringify(openVersion, null, 2)}</pre>
              <button className="btn-secondary" onClick={() => setOpenVersion(null)}>Close</button>
            </div>
          )}
        </div>
      )}

      {comments.length > 0 && (
        <div className="form-card">
          <h3>Review Comment History</h3>
          <ul className="comment-list">
            {comments.map(c => (
              <li key={c.id}>
                <StatusBadge status={c.actionTaken} /> by {c.reviewerName} on v{c.againstVersionNumber} — {new Date(c.createdAt).toLocaleString()}
                <p>{c.comment}</p>
              </li>
            ))}
          </ul>
        </div>
      )}

      {allowReview && report.status === 'Submitted' && (
        <div className="form-card">
          <h3>Review Action</h3>
          {error && <div className="error-banner">{error}</div>}
          <textarea rows={3} placeholder="Comment (required if requesting changes)" value={comment} onChange={e => setComment(e.target.value)} />
          <div className="form-actions">
            <button className="btn-danger" disabled={reviewing} onClick={() => doReview(false)}>Request Changes</button>
            <button className="btn-primary" disabled={reviewing} onClick={() => doReview(true)}>Approve</button>
          </div>
        </div>
      )}
    </div>
  )
}
