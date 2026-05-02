import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { budgetsApi } from '../api/budgets';
import { categoriesApi } from '../api/categories';
import { Budget, Category, CreateBudgetRequest } from '../types';
import BudgetProgress from '../components/Charts/BudgetProgress';
import EmptyState from '../components/Common/EmptyState';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import { getMonthName } from '../utils/formatters';
import './BudgetsPage.css';

/**
 * Budgets page — set and manage monthly spending limits per category.
 * Shows actual spending vs budget with visual progress bars.
 */
const BudgetsPage: React.FC = () => {
  const now = new Date();
  const [year, setYear] = useState(now.getFullYear());
  const [month, setMonth] = useState(now.getMonth() + 1);
  const [budgets, setBudgets] = useState<Budget[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState<Partial<CreateBudgetRequest>>({ month, year });

  const load = () => {
    setLoading(true);
    budgetsApi.getAll(year, month)
      .then(setBudgets)
      .catch(() => toast.error('Failed to load budgets'))
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, [year, month]);
  useEffect(() => { categoriesApi.getAll().then(setCategories); }, []);

  const handleCreate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.categoryId || !form.monthlyLimit) return;
    try {
      await budgetsApi.create({ ...form, month, year } as CreateBudgetRequest);
      toast.success('Budget created');
      setShowForm(false);
      load();
    } catch (err: unknown) {
      toast.error((err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to create budget');
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this budget?')) return;
    try {
      await budgetsApi.delete(id);
      toast.success('Budget deleted');
      load();
    } catch {
      toast.error('Failed to delete budget');
    }
  };

  // Get categories that don't already have a budget this month
  const availableCategories = categories.filter(
    c => !budgets.some(b => b.categoryId === c.id)
  );

  return (
    <div className="budgets-page fade-in">
      {/* Month picker */}
      <div className="budgets-header">
        <div className="budgets-month-picker">
          <select className="form-select" value={month} onChange={e => setMonth(Number(e.target.value))}>
            {Array.from({ length: 12 }, (_, i) => i + 1).map(m => (
              <option key={m} value={m}>{getMonthName(m)}</option>
            ))}
          </select>
          <select className="form-select" value={year} onChange={e => setYear(Number(e.target.value))}>
            {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </select>
        </div>
        <button className="btn btn-primary" onClick={() => setShowForm(true)}>
          + Add Budget
        </button>
      </div>

      {loading ? (
        <div style={{ display: 'flex', justifyContent: 'center', padding: 'var(--space-2xl)' }}>
          <LoadingSpinner />
        </div>
      ) : budgets.length === 0 ? (
        <EmptyState
          icon="🎯"
          title="No budgets set"
          description={`Set spending limits for ${getMonthName(month)} ${year} to stay on track.`}
          action={<button className="btn btn-primary" onClick={() => setShowForm(true)}>Create Budget</button>}
        />
      ) : (
        <div className="budgets-grid">
          {budgets.map(budget => (
            <div key={budget.id} className="card budget-card">
              <div className="budget-card-header">
                <div className="budget-card-title">
                  <span className="budget-dot" style={{ background: budget.categoryColor }} />
                  {budget.categoryName}
                </div>
                <button
                  className="btn btn-secondary"
                  style={{ padding: '4px 10px', fontSize: '0.75rem' }}
                  onClick={() => handleDelete(budget.id)}
                >
                  Delete
                </button>
              </div>
              <BudgetProgress budgets={[budget]} />
            </div>
          ))}
        </div>
      )}

      {/* Add budget modal */}
      {showForm && (
        <div className="modal-overlay" onClick={() => setShowForm(false)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>New Budget — {getMonthName(month)} {year}</h3>
            <form onSubmit={handleCreate}>
              <div className="form-group">
                <label className="form-label">Category</label>
                <select
                  className="form-select"
                  value={form.categoryId ?? ''}
                  onChange={e => setForm(f => ({ ...f, categoryId: Number(e.target.value) }))}
                  required
                >
                  <option value="">Select a category</option>
                  {availableCategories.map(c => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>
              <div className="form-group">
                <label className="form-label">Monthly Limit ($)</label>
                <input
                  type="number"
                  className="form-input"
                  min={1}
                  step={0.01}
                  value={form.monthlyLimit ?? ''}
                  onChange={e => setForm(f => ({ ...f, monthlyLimit: Number(e.target.value) }))}
                  required
                  placeholder="e.g. 500"
                />
              </div>
              <div className="modal-actions">
                <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">Create</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default BudgetsPage;
