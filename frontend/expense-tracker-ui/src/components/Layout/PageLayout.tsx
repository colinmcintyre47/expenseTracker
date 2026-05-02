import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Navbar from './Navbar';
import './PageLayout.css';

/**
 * Shared layout for all authenticated pages.
 * Uses React Router's <Outlet /> to render the active child route.
 *
 * Layout structure:
 * ┌──────────┬─────────────────────────────┐
 * │          │    Navbar (top bar)         │
 * │  Sidebar ├─────────────────────────────┤
 * │          │    <Outlet /> (page content)│
 * └──────────┴─────────────────────────────┘
 */
const PageLayout: React.FC = () => (
  <div className="layout">
    <Sidebar />
    <div className="layout-main">
      <Navbar />
      <main className="layout-content">
        <Outlet />
      </main>
    </div>
  </div>
);

export default PageLayout;
