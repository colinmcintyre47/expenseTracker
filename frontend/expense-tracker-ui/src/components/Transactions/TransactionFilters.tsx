import { useEffect, useState } from 'react';
import { categoriesApi } from '../../api/categories';
import { Category, TransactionFilter } from '../../types';
import './TransactionFilters.css';

interface Props {
  filters: TransactionFilter;
  onChange: (filters: TransactionFilter) => void;
}

/**
 * Filter bar for the transactions page.
 * All fields are optional — the user can combine any combination of filters.
 */
const TransactionFilters: React.FC<Props> = ({ filters, onChange }) => {
  const [categories, setCategories] = useState<Category[]>([]);

  useEffect(() => {
    categoriesApi.getAll().then(setCategories).catch(console.error);
  }, []);

  const update = (patch: Partial<TransactionFilter>) =>
    onChange({ ...filters, ...patch, page: 1 }); // Reset to page 1 on filter change

  return (
    <div className="txn-filters">
      {/* Search */}
      <input
        className="form-input"
        placeholder="Search merchant or description…"
        value={filters.search ?? ''}
        onChange={e => update({ search: e.target.value || undefined })}
      />

      {/* Date range */}
      <input
        type="date"
        className="form-input"
        value={filters.startDate ?? ''}
        onChange={e => update({ startDate: e.target.value || undefined })}
        title="Start date"
      />
      <input
        type="date"
        className="form-input"
        value={filters.endDate ?? ''}
        onChange={e => update({ endDate: e.target.value || undefined })}
        title="End date"
      />

      {/* Category */}
      <select
        className="form-select"
        value={filters.categoryId ?? ''}
        onChange={e => update({ categoryId: e.target.value ? Number(e.target.value) : undefined })}
      >
        <option value="">All Categories</option>
        {categories.map(c => (
          <option key={c.id} value={c.id}>{c.name}</option>
        ))}
      </select>

      {/* Amount range */}
      <input
        type="number"
        className="form-input"
        placeholder="Min $"
        min={0}
        value={filters.minAmount ?? ''}
        onChange={e => update({ minAmount: e.target.value ? Number(e.target.value) : undefined })}
      />
      <input
        type="number"
        className="form-input"
        placeholder="Max $"
        min={0}
        value={filters.maxAmount ?? ''}
        onChange={e => update({ maxAmount: e.target.value ? Number(e.target.value) : undefined })}
      />

      {/* Clear button */}
      <button
        className="btn btn-secondary"
        onClick={() => onChange({ page: 1, pageSize: 50, sortBy: 'date', sortDir: 'desc' })}
      >
        Clear
      </button>
    </div>
  );
};

export default TransactionFilters;
