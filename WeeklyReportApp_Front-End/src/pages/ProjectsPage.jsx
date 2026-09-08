import { useEffect, useState } from 'react'
import api from '../api/client'

export default function ProjectsPage() {
  const [projects, setProjects] = useState([])
  const [form, setForm] = useState({ id: null, name: '', description: '', isActive: true })
  const [error, setError] = useState('')

  const load = () => api.get('/projects', { params: { includeInactive: true } }).then(res => setProjects(res.data))
 useEffect(() => { load() }, [])
 
  const resetForm = () => setForm({ id: null, name: '', description: '', isActive: true })

  const save = async (e) => {
    e.preventDefault()
    setError('')
    try {
      if (form.id) {
        await api.put(`/projects/${form.id}`, { name: form.name, description: form.description, isActive: form.isActive })
      } else {
        await api.post('/projects', { name: form.name, description: form.description, isActive: true })
      }
      resetForm()
      load()
    } catch (err) {
      setError(JSON.stringify(err.response?.data) || 'Failed to save project.')
    }
  }

  const edit = (p) => setForm({ id: p.id, name: p.name, description: p.description || '', isActive: p.isActive })

  const remove = async (id) => {
    if (!confirm('Delete this project? If it has reports, it will be deactivated instead.')) return
    await api.delete(`/projects/${id}`)
    load()
  }

  return (
    <div className="page-container">
      <h1>Projects / Categories</h1>

      <form className="form-card" onSubmit={save}>
        <h3>{form.id ? 'Edit Project' : 'Add New Project'}</h3>
        {error && <div className="error-banner">{error}</div>}
        <div className="form-row">
          <div><label>Name</label><input value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required /></div>
          <div><label>Description</label><input value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} /></div>
          {form.id && (
            <div>
              <label>Active</label>
              <select value={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.value === 'true' })}>
                <option value="true">Active</option>
                <option value="false">Inactive</option>
              </select>
            </div>
          )}
        </div>
        <div className="form-actions">
          {form.id && <button type="button" className="btn-secondary" onClick={resetForm}>Cancel</button>}
          <button type="submit" className="btn-primary">{form.id ? 'Save Changes' : 'Add Project'}</button>
        </div>
      </form>

      <table className="data-table">
        <thead><tr><th>Name</th><th>Description</th><th>Status</th><th></th></tr></thead>
        <tbody>
          {projects.map(p => (
            <tr key={p.id}>
              <td>{p.name}</td>
              <td>{p.description}</td>
              <td>{p.isActive ? 'Active' : 'Inactive'}</td>
              <td><button className="btn-link" onClick={() => edit(p)}>Edit</button> · <button className="btn-link" onClick={() => remove(p.id)}>Delete</button></td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
