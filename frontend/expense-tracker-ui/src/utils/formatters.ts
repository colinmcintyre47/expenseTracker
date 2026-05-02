/**
 * Shared formatting utilities.
 * Centralizing formatters ensures consistent display across all pages.
 */

/**
 * Format a number as US currency.
 * Examples: 1234.5 → "$1,234.50", 0.99 → "$0.99"
 */
export const formatCurrency = (amount: number): string =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(amount);

/**
 * Format a date string for display in transaction tables.
 * Example: "2025-03-10T00:00:00" → "Mar 10, 2025"
 */
export const formatDate = (dateString: string): string => {
  const date = new Date(dateString);
  return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
};

/**
 * Format a date as short month + year.
 * Example: "2025-03-01" → "Mar 2025"
 */
export const formatMonthYear = (year: number, month: number): string =>
  new Date(year, month - 1, 1).toLocaleDateString('en-US', { month: 'long', year: 'numeric' });

/**
 * Format a percentage for display.
 * Example: 81.333 → "81.3%"
 */
export const formatPercent = (value: number, decimals = 1): string =>
  `${value.toFixed(decimals)}%`;

/**
 * Returns a color class based on whether a budget is safe, warning, or exceeded.
 */
export const getBudgetStatusColor = (percentageUsed: number): string => {
  if (percentageUsed >= 100) return 'var(--color-error)';
  if (percentageUsed >= 80) return 'var(--color-warning)';
  return 'var(--color-success)';
};

/**
 * Returns the month name for a month number (1-12).
 */
export const getMonthName = (month: number): string =>
  new Date(2000, month - 1, 1).toLocaleDateString('en-US', { month: 'long' });
