import {
  CartesianGrid, Legend, Line, LineChart,
  ResponsiveContainer, Tooltip, XAxis, YAxis
} from 'recharts';
import { MonthTrend } from '../../types';
import { formatCurrency } from '../../utils/formatters';

interface Props {
  data: MonthTrend[];
}

/** Line chart showing spending and income trends over the last 6 months. */
const MonthlyTrends: React.FC<Props> = ({ data }) => (
  <ResponsiveContainer width="100%" height={280}>
    <LineChart data={data} margin={{ top: 5, right: 20, bottom: 5, left: 10 }}>
      <CartesianGrid strokeDasharray="3 3" stroke="var(--color-border)" />
      <XAxis
        dataKey="monthName"
        tick={{ fontSize: 12, fill: 'var(--color-text-muted)' }}
      />
      <YAxis
        tickFormatter={(v) => `$${(v / 1000).toFixed(0)}k`}
        tick={{ fontSize: 12, fill: 'var(--color-text-muted)' }}
      />
      <Tooltip
        formatter={(value: number) => [formatCurrency(value)]}
        contentStyle={{
          background: 'var(--color-surface)',
          border: '1px solid var(--color-border)',
          borderRadius: '8px',
        }}
      />
      <Legend />
      <Line
        type="monotone"
        dataKey="totalSpent"
        name="Spending"
        stroke="var(--color-error)"
        strokeWidth={2}
        dot={{ fill: 'var(--color-error)', r: 4 }}
        activeDot={{ r: 6 }}
      />
      <Line
        type="monotone"
        dataKey="totalIncome"
        name="Income"
        stroke="var(--color-success)"
        strokeWidth={2}
        dot={{ fill: 'var(--color-success)', r: 4 }}
      />
    </LineChart>
  </ResponsiveContainer>
);

export default MonthlyTrends;
