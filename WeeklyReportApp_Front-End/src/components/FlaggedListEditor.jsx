export default function FlaggedListEditor({ items, onChange, flagField, flagLabel, placeholder, readOnly = false }) {
  const update = (index, value) => {
    const next = [...items]
    next[index] = { ...next[index], description: value }
    onChange(next)
  }

  const setFlag = (index) => {
    onChange(items.map((it, i) => ({ ...it, [flagField]: i === index })))
  }

  const add = () => onChange([...items, { description: '', [flagField]: items.length === 0 }])
  const remove = (index) => onChange(items.filter((_, i) => i !== index))

  return (
    <div className="flagged-list">
      {items.map((item, i) => (
        <div key={i} className="flagged-list-row">
          <input
            type="radio"
            name={flagField}
            checked={!!item[flagField]}
            disabled={readOnly}
            onChange={() => setFlag(i)}
            title={flagLabel}
          />
          <input
            className="flagged-list-input"
            value={item.description}
            placeholder={placeholder}
            disabled={readOnly}
            onChange={e => update(i, e.target.value)}
          />
          {!readOnly && <button type="button" className="btn-danger-sm" onClick={() => remove(i)}>✕</button>}
        </div>
      ))}
      {!readOnly && <button type="button" className="btn-secondary" onClick={add}>+ Add</button>}
      <p className="hint-text">Radio button marks it as the {flagLabel.toLowerCase()} for the week.</p>
    </div>
  )
}
