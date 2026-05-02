import { Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts';
import { CategorySpending } from '../../types';
import { formatCurrency } from '../../utils/formatters';

interface Props {
  data: CategorySpending[];
}

/**
 * Pie chart showing spending breakdown by category.
 * Uses Recharts — a React-native charting library built on D3.
 * ResponsiveContainer makes the chart fill its parent's width automatically.
 */
const SpendingByCategory: React.FC<Props> = ({ data }) => {
  if (!data.length) return <div style={{ color: 'var(--color-text-muted)', textAlign: 'center', padding: '40px' }}>No spending data</div>;

  return (
    <ResponsiveContainer width="100%" height={300}>
      <PieChart>
        <Pie
          data={data}
          dataKey="totalSpent"
          nameKey="categoryName"
          cx="50%"
          cy="50%"
          outerRadius={100}
          innerRadius={50}  /* Donut chart — inner hole makes it easier to read */
          paddingAngle={2}
        >
          {data.map((entry) => (
            <Cell key={entry.categoryId} fill={entry.categoryColor} />
          ))}
        </Pie>
        <Tooltip
          formatter={(value: number) => [formatCurrency(value), 'Spent']}
          contentStyle={{
            background: 'var(--color-surface)',
            border: '1px solid var(--color-border)',
            borderRadius: '8px',
          }}
        />
        <Legend
          formatter={(value) => <span style={{ color: 'var(--color-text-secondary)', fontSize: '0.8rem' }}>{value}</span>}
        />
      </PieChart>
    </ResponsiveContainer>
  );
};

export default SpendingByCategory;
