const COLORS = {
  Draft: '#8b8b8b',
  Submitted: '#2f7fd6',
  NeedsCorrection: '#d9822b',
  Approved: '#2fa84f',
  NotStarted: '#c2c2c2'
}

const LABELS = {
  Draft: 'Draft',
  Submitted: 'Submitted',
  NeedsCorrection: 'Needs Correction',
  Approved: 'Approved',
  NotStarted: 'Not Started'
}

export default function StatusBadge({ status }) {
  const color = COLORS[status] || '#8b8b8b'
  return (
    <span
      style={{
        display: 'inline-block',
        padding: '3px 10px',
        borderRadius: '999px',
        fontSize: '12px',
        fontWeight: 600,
        color: '#fff',
        backgroundColor: color
      }}
    >
      {LABELS[status] || status}
    </span>
  )
}
