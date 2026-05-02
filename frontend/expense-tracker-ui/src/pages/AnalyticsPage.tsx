import { useEffect, useState } from 'react';
import { analyticsApi } from '../api/analytics';
import { MonthlyAnalytics, YearlyAnalytics } from '../types';
import SpendingByCategory from '../components/Charts/SpendingByCategory';
import MonthlyTrends from '../components/Charts/MonthlyTrends';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import { formatCurrency, getMonthName } from '../utils/formatters';
import {
  Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis
} from 'recharts';
import './AnalyticsPage.css';

type View = 'monthly' | 'yearly';

/**
 * Analytics page — deeper financial analysis with monthly and yearly views.
 */
const AnalyticsPage: React.FC = () => {
  const now = new Date();
  const [view, setView] = useState<View>('monthly');
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [monthly, setMonthly] = useState<MonthlyAnalytics | null>(null);
  const [yearly, setYearly] = useState<YearlyAnalytics | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    setLoading(true);
    if (view === 'monthly') {
      analyticsApi.getMonthly(year, month)
        .then(setMonthly)
        .finally(() => setLoading(false));
    } else {
      analyticsApi.getYearly(year)
        .then(setYearly)
        .finally(() => setLoading(false));
    }
  }, [view, year, month]);

  return (
    <div className="analytics-page fade-in">
      {/* View toggle + date selector */}
      <div className="analytics-toolbar">
        <div className="analytics-tabs">
          <button className={`analytics-tab ${view === 'monthly' ? 'active' : ''}`} onClick={() => setView('monthly')}>Monthly</button>
          <button className={`analytics-tab ${view === 'yearly' ? 'active' : ''}`} onClick={() => setView('yearly')}>Yearly</button>
        </div>

        <div className="analytics-selectors">
          {view === 'monthly' && (
            <select className="form-select" value={month} onChange={e => setMonth(Number(e.target.value))}>
              {Array.from({ length: 12 }, (_, i) => i + 1).map(m => (
                <option key={m} value={m}>{getMonthName(m)}</option>
              ))}
            </select>
          )}
          <select className="form-select" value={year} onChange={e => setYear(Number(e.target.value))}>
            {[now.getFullYear() - 2, now.getFullYear() - 1, now.getFullYear()].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
        </div>
      </div>

      {loading ? (
        <div className="analytics-loading"><LoadingSpinner /></div>
      ) : view === 'monthly' && monthly ? (
        <MonthlyView data={monthly} />
      ) : view === 'yearly' && yearly ? (
        <YearlyView data={yearly} />
      ) : null}
    </div>
  );
};

// -------------------------------------------------------------------
// Monthly view sub-component
// -------------------------------------------------------------------
const MonthlyView: React.FC<{ data: MonthlyAnalytics }> = ({ data }) => (
  <div className="analytics-grid">
    {/* Summary stats */}
    <div className="analytics-summary">
      <div className="analytics-stat">
        <div className="analytics-stat-label">Total Spent</div>
        <div className="analytics-stat-value">{formatCurrency(data.totalSpent)}</div>
      </div>
      <div className="analytics-stat">
        <div className="analytics-stat-label">Total Income</div>
        <div className="analytics-stat-value income">{formatCurrency(data.totalIncome)}</div>
      </div>
      <div className="analytics-stat">
        <div className="analytics-stat-label">Transactions</div>
        <div className="analytics-stat-value">{data.transactionCount}</div>
      </div>
      <div className="analytics-stat">
        <div className="analytics-stat-label">Average Transaction</div>
        <div className="analytics-stat-value">{formatCurrency(data.averageTransactionAmount)}</div>
      </div>
    </div>

    {/* Category breakdown */}
    <div className="card analytics-card-wide">
      <h3 className="card-title">Spending by Category</h3>
      <div className="analytics-two-col">
        <SpendingByCategory data={data.categoryBreakdown} />
        <div className="analytics-category-list">
          {data.categoryBreakdown.map(c => (
            <div key={c.categoryId} className="analytics-category-row">
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <span style={{ width: 10, height: 10, borderRadius: '50%', background: c.categoryColor, display: 'inline-block' }} />
                <span style={{ fontSize: '0.875rem' }}>{c.categoryName}</span>
              </div>
              <div style={{ textAlign: 'right' }}>
                <div style={{ fontWeight: 600, fontSize: '0.875rem' }}>{formatCurrency(c.totalSpent)}</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{c.percentage.toFixed(1)}%</div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>

    {/* Top transactions */}
    {data.topTransactions.length > 0 && (
      <div className="card analytics-card-wide">
        <h3 className="card-title">Largest Transactions</h3>
        <table className="analytics-table">
          <thead><tr><th>Merchant</th><th>Category</th><th className="text-right">Amount</th></tr></thead>
          <tbody>
            {data.topTransactions.map(t => (
              <tr key={t.id}>
                <td>{t.merchant}</td>
                <td>
                  <span style={{ background: t.categoryColor + '20', color: t.categoryColor, padding: '2px 8px', borderRadius: '10px', fontSize: '0.75rem', fontWeight: 600 }}>
                    {t.categoryName}
                  </span>
                </td>
                <td className="text-right" style={{ fontWeight: 600 }}>{formatCurrency(t.amount)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    )}
  </div>
);

// -------------------------------------------------------------------
// Yearly view sub-component
// -------------------------------------------------------------------
const YearlyView: React.FC<{ data: YearlyAnalytics }> = ({ data }) => (
  <div className="analytics-grid">
    <div className="analytics-summary">
      <div className="analytics-stat">
        <div className="analytics-stat-label">Total Spent</div>
        <div className="analytics-stat-value">{formatCurrency(data.totalSpent)}</div>
      </div>
      <div className="analytics-stat">
        <div className="analytics-stat-label">Total Income</div>
        <div className="analytics-stat-value income">{formatCurrency(data.totalIncome)}</div>
      </div>
    </div>

    <div className="card analytics-card-wide">
      <h3 className="card-title">Monthly Spending Overview</h3>
      <ResponsiveContainer width="100%" height={280}>
        <BarChart data={data.monthlyBreakdown}>
          <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
          <XAxis dataKey="monthName" tick={{ fontSize: 12 }} />
          <YAxis tickFormatter={v => `$${(v/1000).toFixed(0)}k`} tick={{ fontSize: 12 }} />
          <Tooltip formatter={(v: number) => [formatCurrency(v)]} />
          <Bar dataKey="totalSpent" name="Spending" fill="var(--color-error)" radius={[4,4,0,0]} />
          <Bar dataKey="totalIncome" name="Income" fill="var(--color-success)" radius={[4,4,0,0]} />
        </BarChart>
      </ResponsiveContainer>
    </div>

    <div className="card analytics-card-wide">
      <h3 className="card-title">Yearly Category Breakdown</h3>
      <SpendingByCategory data={data.categoryBreakdown} />
    </div>
  </div>
);

export default AnalyticsPage;
