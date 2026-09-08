const STATUS_OPTIONS = ['NotStarted', 'InProgress', 'Completed', 'Blocked']
const PRIORITY_OPTIONS = ['Low', 'Medium', 'High']

export default function TaskTable({ tasks, onChange, readOnly = false }) {
  const updateTask = (index, field, value) => {
    const next = [...tasks]
    next[index] = { ...next[index], [field]: value }
    onChange(next)
  }

  const addTask = () => {
    onChange([
      ...tasks,
      {
        taskName: '', priority: 'Medium', plannedPercent: 0, actualPercent: 0,
        status: 'NotStarted', timePlannedHours: 0, timeSpentHours: 0, deliverable: ''
      }
    ])
  }

  const removeTask = (index) => onChange(tasks.filter((_, i) => i !== index))

  return (
    <div className="task-table-wrapper">
      <table className="task-table">
        <thead>
          <tr>
            <th>Task Name</th><th>Priority</th><th>Planned %</th><th>Actual %</th>
            <th>Status</th><th>Time Planned (h)</th><th>Time Spent (h)</th><th>Deliverable</th>
            {!readOnly && <th></th>}
          </tr>
        </thead>
        <tbody>
          {tasks.map((t, i) => (
            <tr key={i}>
              <td><input value={t.taskName} disabled={readOnly} onChange={e => updateTask(i, 'taskName', e.target.value)} /></td>
              <td>
                <select value={t.priority} disabled={readOnly} onChange={e => updateTask(i, 'priority', e.target.value)}>
                  {PRIORITY_OPTIONS.map(p => <option key={p} value={p}>{p}</option>)}
                </select>
              </td>
              <td><input type="number" min="0" max="100" value={t.plannedPercent} disabled={readOnly} onChange={e => updateTask(i, 'plannedPercent', Number(e.target.value))} /></td>
              <td><input type="number" min="0" max="100" value={t.actualPercent} disabled={readOnly} onChange={e => updateTask(i, 'actualPercent', Number(e.target.value))} /></td>
              <td>
                <select value={t.status} disabled={readOnly} onChange={e => updateTask(i, 'status', e.target.value)}>
                  {STATUS_OPTIONS.map(s => <option key={s} value={s}>{s}</option>)}
                </select>
              </td>
              <td><input type="number" min="0" step="0.5" value={t.timePlannedHours} disabled={readOnly} onChange={e => updateTask(i, 'timePlannedHours', Number(e.target.value))} /></td>
              <td><input type="number" min="0" step="0.5" value={t.timeSpentHours} disabled={readOnly} onChange={e => updateTask(i, 'timeSpentHours', Number(e.target.value))} /></td>
              <td><input value={t.deliverable || ''} disabled={readOnly} onChange={e => updateTask(i, 'deliverable', e.target.value)} /></td>
              {!readOnly && <td><button type="button" className="btn-danger-sm" onClick={() => removeTask(i)}>✕</button></td>}
            </tr>
          ))}
        </tbody>
      </table>
      {!readOnly && <button type="button" className="btn-secondary" onClick={addTask}>+ Add Task</button>}
    </div>
  )
}
