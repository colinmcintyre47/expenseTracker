/**
 * Shared TypeScript types used throughout the frontend.
 * These mirror the DTO classes from the backend.
 * Keeping them in one file makes it easy to find and update them.
 */

// =====================================================================
// AUTH
// =====================================================================
export interface AuthResponse {
  token: string;
  userId: number;
  email: string;
  firstName: string;
  lastName: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

// =====================================================================
// CATEGORIES
// =====================================================================
export interface Category {
  id: number;
  name: string;
  color: string;
  icon: string;
  isSystem: boolean;
}

// =====================================================================
// TRANSACTIONS
// =====================================================================
export interface Transaction {
  id: number;
  date: string; // ISO date string from JSON
  merchant: string;
  description: string;
  amount: number;
  transactionType: 'Debit' | 'Credit';
  accountName: string;
  isAnomaly: boolean;
  createdAt: string;
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  categoryIcon: string;
}

export interface TransactionFilter {
  startDate?: string;
  endDate?: string;
  categoryId?: number;
  minAmount?: number;
  maxAmount?: number;
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDir?: 'asc' | 'desc';
}

export interface PagedResult<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// =====================================================================
// ANALYTICS
// =====================================================================
export interface CategorySpending {
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  categoryIcon: string;
  totalSpent: number;
  percentage: number;
}

export interface TopTransaction {
  id: number;
  merchant: string;
  amount: number;
  date: string;
  categoryName: string;
  categoryColor: string;
}

export interface MonthTrend {
  year: number;
  month: number;
  monthName: string;
  totalSpent: number;
  totalIncome: number;
}

export interface DashboardSummary {
  totalSpentThisMonth: number;
  totalSpentLastMonth: number;
  monthOverMonthChange: number;
  transactionCountThisMonth: number;
  unreadAlertCount: number;
  spendingByCategory: CategorySpending[];
  largestTransactions: TopTransaction[];
  recentTransactions: TopTransaction[];
  spendingTrend: MonthTrend[];
}

export interface MonthlyAnalytics {
  year: number;
  month: number;
  monthName: string;
  totalSpent: number;
  totalIncome: number;
  netAmount: number;
  transactionCount: number;
  averageTransactionAmount: number;
  categoryBreakdown: CategorySpending[];
  topTransactions: TopTransaction[];
}

export interface MonthSummary {
  month: number;
  monthName: string;
  totalSpent: number;
  totalIncome: number;
}

export interface YearlyAnalytics {
  year: number;
  totalSpent: number;
  totalIncome: number;
  monthlyBreakdown: MonthSummary[];
  categoryBreakdown: CategorySpending[];
}

// =====================================================================
// BUDGETS
// =====================================================================
export interface Budget {
  id: number;
  categoryId: number;
  categoryName: string;
  categoryColor: string;
  categoryIcon: string;
  monthlyLimit: number;
  month: number;
  year: number;
  amountSpent: number;
  remaining: number;
  percentageUsed: number;
}

export interface CreateBudgetRequest {
  categoryId: number;
  monthlyLimit: number;
  month: number;
  year: number;
}

// =====================================================================
// ALERTS
// =====================================================================
export type AlertType = 'BudgetWarning' | 'BudgetExceeded' | 'Anomaly' | 'NewMerchant';

export interface Alert {
  id: number;
  transactionId?: number;
  type: AlertType;
  message: string;
  isRead: boolean;
  createdAt: string;
}

// =====================================================================
// STATEMENTS
// =====================================================================
export interface UploadedStatement {
  id: number;
  bankName: string;
  fileName: string;
  uploadedAt: string;
  transactionCount: number;
  status: 'Processing' | 'Completed' | 'Failed';
}

export interface UploadResult {
  statementId: number;
  importedCount: number;
  duplicateCount: number;
  errorCount: number;
  errors: string[];
}
