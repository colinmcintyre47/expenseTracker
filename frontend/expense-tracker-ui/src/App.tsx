import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { Toaster } from 'react-hot-toast';
import { AuthProvider, useAuth } from './context/AuthContext';
import PageLayout from './components/Layout/PageLayout';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import TransactionsPage from './pages/TransactionsPage';
import UploadPage from './pages/UploadPage';
import BudgetsPage from './pages/BudgetsPage';
import AnalyticsPage from './pages/AnalyticsPage';
import SettingsPage from './pages/SettingsPage';

/**
 * ProtectedRoute wraps routes that require authentication.
 * If the user is not logged in, they're redirected to /login.
 * Shows nothing during the initial auth check (isLoading) to prevent flicker.
 */
const ProtectedRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) return null; // Prevent flash of login page on refresh
  if (!isAuthenticated) return <Navigate to="/login" replace />;

  return <>{children}</>;
};

/**
 * App component sets up routing.
 * Public routes: /login, /register
 * Private routes: everything else (wrapped in ProtectedRoute + PageLayout)
 */
const AppRoutes: React.FC = () => {
  const { isAuthenticated } = useAuth();

  return (
    <Routes>
      {/* Public routes */}
      <Route path="/login" element={isAuthenticated ? <Navigate to="/" replace /> : <LoginPage />} />
      <Route path="/register" element={isAuthenticated ? <Navigate to="/" replace /> : <RegisterPage />} />

      {/* Protected routes — all wrapped in the shared sidebar layout */}
      <Route
        path="/"
        element={
          <ProtectedRoute>
            <PageLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<DashboardPage />} />
        <Route path="transactions" element={<TransactionsPage />} />
        <Route path="upload" element={<UploadPage />} />
        <Route path="budgets" element={<BudgetsPage />} />
        <Route path="analytics" element={<AnalyticsPage />} />
        <Route path="settings" element={<SettingsPage />} />
      </Route>

      {/* Fallback — redirect unknown routes to home */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
};

const App: React.FC = () => (
  <BrowserRouter>
    <AuthProvider>
      <AppRoutes />
      {/* Global toast notifications — accessible from any page via react-hot-toast */}
      <Toaster
        position="top-right"
        toastOptions={{
          duration: 4000,
          style: {
            background: 'var(--color-surface)',
            color: 'var(--color-text)',
            border: '1px solid var(--color-border)',
            boxShadow: 'var(--shadow-md)',
          },
        }}
      />
    </AuthProvider>
  </BrowserRouter>
);

export default App;
