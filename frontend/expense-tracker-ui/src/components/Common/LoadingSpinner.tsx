import './LoadingSpinner.css';

interface Props { size?: 'sm' | 'md' | 'lg'; }

/** Animated loading spinner for async operations. */
const LoadingSpinner: React.FC<Props> = ({ size = 'md' }) => (
  <div className={`spinner spinner-${size}`} role="status" aria-label="Loading">
    <span className="sr-only">Loading…</span>
  </div>
);

export default LoadingSpinner;
