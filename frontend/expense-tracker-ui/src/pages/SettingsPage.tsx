import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { alertsApi } from '../api/alerts';
import { categoriesApi } from '../api/categories';
import { Alert, AlertType, Category } from '../types';
import { formatDate } from '../utils/formatters';
import './SettingsPage.css';

/**
 * Settings page — manage categories and view/dismiss alerts.
 * Future: account settings, notification preferences, theme toggle.
 */
const SettingsPage: React.FC = () => {
  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [activeTab, setActiveTab] = useState<'alerts' | 'categories'>('alerts');

  const loadAlerts = () => alertsApi.getAll().then(setAlerts).catch(console.error);
  const loadCategories = () => categoriesApi.getAll().then(setCategories).catch(console.error);

  useEffect(() => { loadAlerts(); loadCategories(); }, []);

  const handleMarkRead = async (id: number) => {
    await alertsApi.markRead(id);
    loadAlerts();
  };

  const handleMarkAllRead = async () => {
    await alertsApi.markAllRead();
    toast.success('All alerts dismissed');
    loadAlerts();
  };

  const handleDeleteCategory = async (id: number) => {
    if (!confirm('Delete this custom category?')) return;
    try {
      await categoriesApi.delete(id);
      toast.success('Category deleted');
      loadCategories();
    } catch {
      toast.error('Cannot delete this category');
    }
  };

  const ALERT_ICONS: Record<AlertType, string> = {
    BudgetWarning: '⚠',
    BudgetExceeded: '🚨',
    Anomaly: '🔍',
    NewMerchant: '🆕',
  };

  return (
    <div className="settings-page fade-in">
      {/* Tabs */}
      <div className="settings-tabs">
        <button className={`settings-tab ${activeTab === 'alerts' ? 'active' : ''}`} onClick={() => setActiveTab('alerts')}>
          Alerts {alerts.filter(a => !a.isRead).length > 0 && (
            <span className="badge badge-error">{alerts.filter(a => !a.isRead).length}</span>
          )}
        </button>
        <button className={`settings-tab ${activeTab === 'categories' ? 'active' : ''}`} onClick={() => setActiveTab('categories')}>
          Categories
        </button>
      </div>

      {activeTab === 'alerts' && (
        <div className="card">
          <div className="settings-section-header">
            <h3>Alerts</h3>
            {alerts.some(a => !a.isRead) && (
              <button className="btn btn-secondary" onClick={handleMarkAllRead}>Dismiss All</button>
            )}
          </div>

          {alerts.length === 0 ? (
            <div style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: 'var(--space-xl)' }}>
              No alerts — everything looks good!
            </div>
          ) : (
            <div className="alerts-list">
              {alerts.map(alert => (
                <div key={alert.id} className={`alert-item ${alert.isRead ? 'read' : ''}`}>
                  <span className="alert-icon">{ALERT_ICONS[alert.type] ?? '🔔'}</span>
                  <div className="alert-content">
                    <div className="alert-message">{alert.message}</div>
                    <div className="alert-time">{formatDate(alert.createdAt)}</div>
                  </div>
                  {!alert.isRead && (
                    <button className="btn btn-secondary" style={{ fontSize: '0.75rem', padding: '4px 10px' }} onClick={() => handleMarkRead(alert.id)}>
                      Dismiss
                    </button>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {activeTab === 'categories' && (
        <div className="card">
          <div className="settings-section-header">
            <h3>Categories</h3>
          </div>
          <div className="categories-list">
            {categories.map(cat => (
              <div key={cat.id} className="category-item">
                <div className="category-item-left">
                  <span className="category-color" style={{ background: cat.color }} />
                  <span className="category-name">{cat.name}</span>
                  {cat.isSystem && <span className="badge badge-neutral">System</span>}
                </div>
                {!cat.isSystem && (
                  <button
                    className="btn btn-secondary"
                    style={{ fontSize: '0.75rem', padding: '4px 10px', color: 'var(--color-error)' }}
                    onClick={() => handleDeleteCategory(cat.id)}
                  >
                    Delete
                  </button>
                )}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
};

export default SettingsPage;
