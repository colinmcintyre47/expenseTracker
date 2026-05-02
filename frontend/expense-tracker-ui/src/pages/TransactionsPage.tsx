import { useCallback, useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { transactionsApi } from '../api/transactions';
import { categoriesApi } from '../api/categories';
import { Category, PagedResult, Transaction, TransactionFilter } from '../types';
import TransactionTable from '../components/Transactions/TransactionTable';
import TransactionFilters from '../components/Transactions/TransactionFilters';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import EmptyState from '../components/Common/EmptyState';
import './TransactionsPage.css';

/**
 * Transactions page — searchable, filterable, paginated list of all transactions.
 * Supports re-categorizing and deleting individual transactions.
 */
const TransactionsPage: React.FC = () => {
  const [result, setResult] = useState<PagedResult<Transaction> | null>(null);
  const [filters, setFilters] = useState<TransactionFilter>({ page: 1, pageSize: 50, sortBy: 'date', sortDir: 'desc' });
  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState<Category[]>([]);

  // Modal state for re-categorizing
  const [recategorizeTarget, setRecategorizeTarget] = useState<Transaction | null>(null);
  const [newCategoryId, setNewCategoryId] = useState('');

  const load = useCallback(() => {
    setLoading(true);
    transactionsApi.getAll(filters)
      .then(setResult)
      .catch(() => toast.error('Failed to load transactions'))
      .finally(() => setLoading(false));
  }, [filters]);

  useEffect(() => { load(); }, [load]);

  useEffect(() => {
    categoriesApi.getAll().then(setCategories).catch(console.error);
  }, []);

  const handleDelete = async (id: number) => {
    if (!confirm('Delete this transaction?')) return;
    try {
      await transactionsApi.delete(id);
      toast.success('Transaction deleted');
      load();
    } catch {
      toast.error('Failed to delete transaction');
    }
  };

  const handleRecategorize = async () => {
    if (!recategorizeTarget || !newCategoryId) return;
    try {
      await transactionsApi.updateCategory(recategorizeTarget.id, Number(newCategoryId));
      toast.success('Category updated');
      setRecategorizeTarget(null);
      load();
    } catch {
      toast.error('Failed to update category');
    }
  };

  return (
    <div className="txn-page fade-in">
      <TransactionFilters filters={filters} onChange={setFilters} />

      {loading ? (
        <div className="txn-page-loading"><LoadingSpinner /></div>
      ) : !result?.data.length ? (
        <EmptyState
          icon="🔍"
          title={filters.search ? 'No transactions match your search' : 'No transactions found'}
          description="Try adjusting your filters or upload a bank statement."
        />
      ) : (
        <>
          <div className="txn-page-meta">
            Showing {result.data.length} of {result.totalCount} transactions
          </div>

          <div className="card" style={{ padding: 0 }}>
            <TransactionTable
              transactions={result.data}
              onDelete={handleDelete}
              onRecategorize={t => { setRecategorizeTarget(t); setNewCategoryId(String(t.categoryId)); }}
            />
          </div>

          {/* Pagination */}
          {result.totalPages > 1 && (
            <div className="txn-pagination">
              <button
                className="btn btn-secondary"
                disabled={filters.page === 1}
                onClick={() => setFilters(f => ({ ...f, page: (f.page ?? 1) - 1 }))}
              >
                ← Previous
              </button>
              <span className="txn-pagination-info">
                Page {filters.page} of {result.totalPages}
              </span>
              <button
                className="btn btn-secondary"
                disabled={filters.page === result.totalPages}
                onClick={() => setFilters(f => ({ ...f, page: (f.page ?? 1) + 1 }))}
              >
                Next →
              </button>
            </div>
          )}
        </>
      )}

      {/* Re-categorize modal */}
      {recategorizeTarget && (
        <div className="modal-overlay" onClick={() => setRecategorizeTarget(null)}>
          <div className="modal" onClick={e => e.stopPropagation()}>
            <h3>Change Category</h3>
            <p className="modal-desc">{recategorizeTarget.merchant}</p>
            <select
              className="form-select"
              value={newCategoryId}
              onChange={e => setNewCategoryId(e.target.value)}
            >
              {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
            </select>
            <div className="modal-actions">
              <button className="btn btn-secondary" onClick={() => setRecategorizeTarget(null)}>Cancel</button>
              <button className="btn btn-primary" onClick={handleRecategorize}>Save</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default TransactionsPage;
