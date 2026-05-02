import { Transaction } from '../../types';
import { formatCurrency, formatDate } from '../../utils/formatters';
import './TransactionTable.css';

interface Props {
  transactions: Transaction[];
  onDelete?: (id: number) => void;
  onRecategorize?: (transaction: Transaction) => void;
}

/**
 * Tabular view of transactions.
 * Shows date, merchant, category badge, amount, and action buttons.
 * Anomalous transactions are highlighted with a warning indicator.
 */
const TransactionTable: React.FC<Props> = ({ transactions, onDelete, onRecategorize }) => (
  <div className="txn-table-wrapper">
    <table className="txn-table">
      <thead>
        <tr>
          <th>Date</th>
          <th>Merchant</th>
          <th>Category</th>
          <th>Type</th>
          <th className="text-right">Amount</th>
          {(onDelete || onRecategorize) && <th></th>}
        </tr>
      </thead>
      <tbody>
        {transactions.map(txn => (
          <tr key={txn.id} className={txn.isAnomaly ? 'txn-anomaly' : ''}>
            <td className="txn-date">{formatDate(txn.date)}</td>
            <td>
              <div className="txn-merchant">{txn.merchant || txn.description}</div>
              {txn.isAnomaly && (
                <span className="txn-anomaly-badge" title="Unusual transaction detected">
                  ⚠ Unusual
                </span>
              )}
            </td>
            <td>
              <span
                className="txn-category"
                style={{ background: txn.categoryColor + '20', color: txn.categoryColor }}
              >
                {txn.categoryName}
              </span>
            </td>
            <td>
              <span className={`badge ${txn.transactionType === 'Credit' ? 'badge-success' : 'badge-neutral'}`}>
                {txn.transactionType}
              </span>
            </td>
            <td className="text-right">
              <span className={`txn-amount ${txn.transactionType === 'Credit' ? 'amount-credit' : 'amount-debit'}`}>
                {txn.transactionType === 'Credit' ? '+' : '-'}{formatCurrency(txn.amount)}
              </span>
            </td>
            {(onDelete || onRecategorize) && (
              <td>
                <div className="txn-actions">
                  {onRecategorize && (
                    <button
                      className="txn-action-btn"
                      onClick={() => onRecategorize(txn)}
                      title="Change category"
                    >
                      ✏
                    </button>
                  )}
                  {onDelete && (
                    <button
                      className="txn-action-btn danger"
                      onClick={() => onDelete(txn.id)}
                      title="Delete transaction"
                    >
                      ✕
                    </button>
                  )}
                </div>
              </td>
            )}
          </tr>
        ))}
      </tbody>
    </table>
  </div>
);

export default TransactionTable;
