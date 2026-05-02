import { Budget } from '../../types';
import { formatCurrency, getBudgetStatusColor } from '../../utils/formatters';
import './BudgetProgress.css';

interface Props {
  budgets: Budget[];
}

/**
 * Visual budget progress bars showing each category's spending vs limit.
 * Color changes from green → orange → red as the limit approaches/exceeds.
 */
const BudgetProgress: React.FC<Props> = ({ budgets }) => (
  <div className="budget-list">
    {budgets.map(budget => {
      const pct = Math.min(budget.percentageUsed, 100); // Cap bar at 100% visually
      const color = getBudgetStatusColor(budget.percentageUsed);

      return (
        <div key={budget.id} className="budget-item">
          <div className="budget-item-header">
            <div className="budget-item-name">
              <span
                className="budget-dot"
                style={{ background: budget.categoryColor }}
              />
              {budget.categoryName}
            </div>
            <div className="budget-item-amounts">
              <span style={{ color }}>{formatCurrency(budget.amountSpent)}</span>
              <span className="budget-limit">/ {formatCurrency(budget.monthlyLimit)}</span>
            </div>
          </div>

          {/* Progress bar */}
          <div className="budget-bar-track">
            <div
              className="budget-bar-fill"
              style={{ width: `${pct}%`, background: color }}
            />
          </div>

          <div className="budget-item-footer">
            <span style={{ color, fontSize: '0.75rem', fontWeight: 600 }}>
              {budget.percentageUsed.toFixed(0)}% used
            </span>
            <span style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
              {budget.remaining >= 0
                ? `${formatCurrency(budget.remaining)} remaining`
                : `${formatCurrency(Math.abs(budget.remaining))} over budget`}
            </span>
          </div>
        </div>
      );
    })}
  </div>
);

export default BudgetProgress;
