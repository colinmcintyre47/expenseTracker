import { NavLink } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import './Sidebar.css';

/** Navigation items configuration — easy to add new pages here. */
const NAV_ITEMS = [
  { path: '/',             label: 'Dashboard',     icon: '◈' },
  { path: '/transactions', label: 'Transactions',  icon: '↕' },
  { path: '/upload',       label: 'Upload',        icon: '↑' },
  { path: '/budgets',      label: 'Budgets',       icon: '◎' },
  { path: '/analytics',    label: 'Analytics',     icon: '◉' },
  { path: '/settings',     label: 'Settings',      icon: '⚙' },
];

const Sidebar: React.FC = () => {
  const { user, logout } = useAuth();

  return (
    <aside className="sidebar">
      {/* App logo / brand */}
      <div className="sidebar-brand">
        <span className="sidebar-brand-icon">$</span>
        <span className="sidebar-brand-text">ExpenseTracker</span>
      </div>

      {/* Navigation links */}
      <nav className="sidebar-nav">
        {NAV_ITEMS.map(item => (
          <NavLink
            key={item.path}
            to={item.path}
            end={item.path === '/'} // "end" prevents Dashboard from being active on all routes
            className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
          >
            <span className="sidebar-link-icon">{item.icon}</span>
            <span>{item.label}</span>
          </NavLink>
        ))}
      </nav>

      {/* User info and logout at the bottom */}
      <div className="sidebar-footer">
        <div className="sidebar-user">
          <div className="sidebar-avatar">
            {user?.firstName?.[0]}{user?.lastName?.[0]}
          </div>
          <div className="sidebar-user-info">
            <div className="sidebar-user-name">{user?.firstName} {user?.lastName}</div>
            <div className="sidebar-user-email">{user?.email}</div>
          </div>
        </div>
        <button className="sidebar-logout" onClick={logout} title="Sign out">
          ⎋
        </button>
      </div>
    </aside>
  );
};

export default Sidebar;
