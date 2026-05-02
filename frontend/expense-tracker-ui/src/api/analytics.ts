import { DashboardSummary, MonthlyAnalytics, YearlyAnalytics } from '../types';
import client from './client';

export const analyticsApi = {
  getDashboard: () =>
    client.get<DashboardSummary>('/analytics/dashboard').then(r => r.data),

  getMonthly: (year?: number, month?: number) =>
    client.get<MonthlyAnalytics>('/analytics/monthly', { params: { year, month } }).then(r => r.data),

  getYearly: (year?: number) =>
    client.get<YearlyAnalytics>('/analytics/yearly', { params: { year } }).then(r => r.data),
};
