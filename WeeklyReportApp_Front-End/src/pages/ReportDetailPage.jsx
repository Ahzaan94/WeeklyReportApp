import { useParams } from 'react-router-dom'
import ReportView from '../components/ReportView'

export default function ReportDetailPage() {
  const { id } = useParams()
  return (
    <div className="page-container">
      <ReportView reportId={id} allowReview={false} />
    </div>
  )
}
