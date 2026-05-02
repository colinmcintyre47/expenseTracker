import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { analyticsApi } from '../api/analytics';
import { DashboardSummary } from '../types';
import StatCard from '../components/Common/StatCard';
import SpendingByCategory from '../components/Charts/SpendingByCategory';
import MonthlyTrends from '../components/Charts/MonthlyTrends';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import EmptyState from '../components/Common/EmptyState';
import { formatCurrency, formatDate } from '../utils/formatters';
import './DashboardPage.css';

/**
 * Dashboard — the first page users see after login.
 * Shows a high-level financial summary for the current month.
 */
const DashboardPage: React.FC = () => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    analyticsApi.getDashboard()
      .then(setSummary)
      .catch(() => setError('Failed to load dashboard data'))
      .finally(() => setLoading(false));
  }, []);

  if (loading) return (
    <div className="dashboard-loading">
      <LoadingSpinner size="lg" />
    </div>
  );

  if (error) return <div className="dashboard-error">{error}</div>;

  const hasData = summary && summary.transactionCountThisMonth > 0;

  return (
    <div className="dashboard fade-in">
      {/* Stat Cards Row */}
      <div className="dashboard-stats">
        <StatCard
          title="Spent This Month"
          value={formatCurrency(summary?.totalSpentThisMonth ?? 0)}
          trend={summary?.monthOverMonthChange}
          accentColor="var(--color-error)"
        />
        <StatCard
          title="Transactions"
          value={String(summary?.transactionCountThisMonth ?? 0)}
          subtitle="This month"
          accentColor="var(--color-primary)"
        />
        <StatCard
          title="Spent Last Month"
          value={formatCurrency(summary?.totalSpentLastMonth ?? 0)}
          accentColor="var(--color-warning)"
        />
        <StatCard
          title="Unread Alerts"
          value={String(summary?.unreadAlertCount ?? 0)}
          subtitle={summary?.unreadAlertCount ? 'Requires attention' : 'All clear'}
          accentColor="var(--color-success)"
        />
      </div>

      {!hasData ? (
        <EmptyState
          icon="📊"
          title="No transactions yet"
          description="Upload a bank statement to see your spending dashboard."
          action={<Link to="/upload" className="btn btn-primary">Upload Statement</Link>}
        />
      ) : (
        <div className="dashboard-grid">
          {/* Spending trend line chart */}
          <div className="card dashboard-card-wide">
            <h3 className="card-title">6-Month Spending Trend</h3>
            <MonthlyTrends data={summary.spendingTrend} />
          </div>

          {/* Category pie chart */}
          <div className="card">
            <h3 className="card-title">Spending by Category</h3>
            <SpendingByCategory data={summary.spendingByCategory} />
          </div>

          {/* Largest transactions */}
          <div className="card">
            <h3 className="card-title">Largest Transactions</h3>
            <div className="txn-mini-list">
              {summary.largestTransactions.map(txn => (
                <div key={txn.id} className="txn-mini-item">
                  <div>
                    <div className="txn-mini-merchant">{txn.merchant}</div>
                    <div className="txn-mini-date">{formatDate(txn.date)}</div>
                  </div>
                  <span className="txn-mini-amount">{formatCurrency(txn.amount)}</span>
                </div>
              ))}
            </div>
            <Link to="/transactions" className="card-link">View all →</Link>
          </div>

          {/* Recent activity */}
          <div className="card">
            <h3 className="card-title">Recent Activity</h3>
            <div className="txn-mini-list">
              {summary.recentTransactions.slice(0, 8).map(txn => (
                <div key={txn.id} className="txn-mini-item">
                  <div>
                    <div className="txn-mini-merchant">{txn.merchant}</div>
                    <div className="txn-mini-meta">
                      <span
                        style={{
                          background: txn.categoryColor + '20',
                          color: txn.categoryColor,
                          padding: '1px 6px',
                          borderRadius: '10px',
                          fontSize: '0.7rem',
                          fontWeight: 600
                        }}
                      >
                        {txn.categoryName}
                      </span>
                    </div>
                  </div>
                  <span className="txn-mini-amount">{formatCurrency(txn.amount)}</span>
                </div>
              ))}
            </div>
            <Link to="/transactions" className="card-link">View all →</Link>
          </div>
        </div>
      )}
    </div>
  );
};

export default DashboardPage;
