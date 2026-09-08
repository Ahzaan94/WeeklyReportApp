import { useNavigate, useParams } from 'react-router-dom'
import ReportView from '../components/ReportView'

export default function ManagerReviewPage() {
  const { id } = useParams()
  const navigate = useNavigate()

  return (
    <div className="page-container">
      <ReportView reportId={id} allowReview={true} onReviewed={() => navigate('/team-reports')} />
    </div>
  )
}
