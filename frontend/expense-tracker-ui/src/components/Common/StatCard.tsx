import './StatCard.css';

interface StatCardProps {
  title: string;
  value: string;
  subtitle?: string;
  /** Optional color to accent the card (hex value) */
  accentColor?: string;
  /** Optional trend indicator: positive = green, negative = red */
  trend?: number;
}

/**
 * Summary statistic card used in the dashboard header row.
 * Example: "Total Spent This Month — $1,234.56 ↑ 12% vs last month"
 */
const StatCard: React.FC<StatCardProps> = ({ title, value, subtitle, accentColor, trend }) => (
  <div className="stat-card" style={accentColor ? { borderTopColor: accentColor } : {}}>
    <div className="stat-card-title">{title}</div>
    <div className="stat-card-value">{value}</div>
    {subtitle && <div className="stat-card-subtitle">{subtitle}</div>}
    {trend !== undefined && (
      <div className={`stat-card-trend ${trend >= 0 ? 'up' : 'down'}`}>
        {trend >= 0 ? '↑' : '↓'} {Math.abs(trend).toFixed(1)}% vs last month
      </div>
    )}
  </div>
);

export default StatCard;
