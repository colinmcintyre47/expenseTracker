import './EmptyState.css';

interface Props {
  icon?: string;
  title: string;
  description?: string;
  action?: React.ReactNode;
}

/** Shown when a list has no items — gives users guidance on what to do next. */
const EmptyState: React.FC<Props> = ({ icon = '📭', title, description, action }) => (
  <div className="empty-state">
    <div className="empty-state-icon">{icon}</div>
    <h3 className="empty-state-title">{title}</h3>
    {description && <p className="empty-state-desc">{description}</p>}
    {action && <div className="empty-state-action">{action}</div>}
  </div>
);

export default EmptyState;
