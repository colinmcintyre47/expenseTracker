import { useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { alertsApi } from '../../api/alerts';
import './Navbar.css';

/** Maps route paths to human-readable page titles. */
const PAGE_TITLES: Record<string, string> = {
  '/': 'Dashboard',
  '/transactions': 'Transactions',
  '/upload': 'Upload Statement',
  '/budgets': 'Budgets',
  '/analytics': 'Analytics',
  '/settings': 'Settings',
};

/**
 * Top navigation bar showing the current page title and unread alert count.
 * The alert badge refreshes every 60 seconds to catch new alerts.
 */
const Navbar: React.FC = () => {
  const location = useLocation();
  const [unreadCount, setUnreadCount] = useState(0);

  const title = PAGE_TITLES[location.pathname] ?? 'Expense Tracker';

  useEffect(() => {
    // Fetch unread alert count and refresh periodically
    const fetchAlerts = async () => {
      try {
        const alerts = await alertsApi.getAll(true); // unreadOnly = true
        setUnreadCount(alerts.length);
      } catch {
        // Non-critical — don't show error for background refresh
      }
    };

    fetchAlerts();
    const interval = setInterval(fetchAlerts, 60_000);
    return () => clearInterval(interval);
  }, [location.pathname]); // Re-check when navigating

  return (
    <header className="navbar">
      <h1 className="navbar-title">{title}</h1>
      <div className="navbar-actions">
        {unreadCount > 0 && (
          <div className="navbar-alert-badge" title={`${unreadCount} unread alerts`}>
            <span>🔔</span>
            <span className="badge badge-error">{unreadCount}</span>
          </div>
        )}
      </div>
    </header>
  );
};

export default Navbar;
