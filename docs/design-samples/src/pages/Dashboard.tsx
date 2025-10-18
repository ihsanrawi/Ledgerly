import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ArrowUp, ArrowDown, TrendingUp, DollarSign } from "lucide-react";
import {
  LineChart,
  Line,
  BarChart,
  Bar,
  PieChart,
  Pie,
  Cell,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";

const netWorthData = {
  totalAssets: 125430.50,
  totalLiabilities: 45280.00,
  netWorth: 80150.50,
  trend: 5.2,
};

const cashFlowData = [
  { date: "Feb 1", balance: 8340 },
  { date: "Feb 5", balance: 8100 },
  { date: "Feb 10", balance: 9500 },
  { date: "Feb 15", balance: 9300 },
  { date: "Feb 20", balance: 9800 },
  { date: "Feb 25", balance: 9600 },
  { date: "Mar 1", balance: 10200 },
  { date: "Mar 5", balance: 10000 },
];

const expenseData = [
  { name: "Groceries", value: 450, color: "hsl(var(--chart-1))" },
  { name: "Dining", value: 230, color: "hsl(var(--chart-2))" },
  { name: "Transport", value: 180, color: "hsl(var(--chart-3))" },
  { name: "Entertainment", value: 150, color: "hsl(var(--chart-4))" },
  { name: "Utilities", value: 120, color: "hsl(var(--chart-5))" },
];

const incomeExpenseData = [
  { month: "Sep", income: 5200, expenses: 3800 },
  { month: "Oct", income: 5200, expenses: 3900 },
  { month: "Nov", income: 5200, expenses: 3700 },
  { month: "Dec", income: 5200, expenses: 4200 },
  { month: "Jan", income: 5200, expenses: 3600 },
  { month: "Feb", income: 5200, expenses: 3500 },
];

const recentTransactions = [
  { id: 1, date: "2025-02-01", payee: "Whole Foods", amount: -85.23, category: "Groceries" },
  { id: 2, date: "2025-02-01", payee: "Shell Gas", amount: -45.00, category: "Transport" },
  { id: 3, date: "2025-02-02", payee: "Netflix", amount: -15.99, category: "Entertainment" },
  { id: 4, date: "2025-02-03", payee: "Salary Deposit", amount: 5200.00, category: "Income" },
  { id: 5, date: "2025-02-04", payee: "Electric Company", amount: -120.45, category: "Utilities" },
];

export default function Dashboard() {
  return (
    <div className="p-6 space-y-6 animate-fade-in">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Dashboard</h1>
        <p className="text-muted-foreground mt-1">Your financial overview at a glance</p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Net Worth Summary */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <DollarSign className="h-5 w-5 text-accent" />
              Net Worth
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm text-muted-foreground">Total Assets</p>
              <p className="text-2xl font-mono font-semibold text-success">
                ${netWorthData.totalAssets.toLocaleString()}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Total Liabilities</p>
              <p className="text-2xl font-mono font-semibold text-destructive">
                ${netWorthData.totalLiabilities.toLocaleString()}
              </p>
            </div>
            <div className="pt-4 border-t">
              <p className="text-sm text-muted-foreground">Net Worth</p>
              <div className="flex items-center gap-2">
                <p className="text-3xl font-mono font-bold text-accent">
                  ${netWorthData.netWorth.toLocaleString()}
                </p>
                <div className="flex items-center text-success">
                  <ArrowUp className="h-4 w-4" />
                  <span className="text-sm font-medium">{netWorthData.trend}%</span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Cash Flow Timeline - Prominent */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <TrendingUp className="h-5 w-5 text-accent" />
              Cash Flow Prediction
            </CardTitle>
            <CardDescription>Next 30 days balance forecast</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={250}>
              <LineChart data={cashFlowData}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="date" stroke="hsl(var(--muted-foreground))" />
                <YAxis stroke="hsl(var(--muted-foreground))" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(var(--popover))",
                    border: "1px solid hsl(var(--border))",
                    borderRadius: "var(--radius)",
                  }}
                />
                <Line
                  type="monotone"
                  dataKey="balance"
                  stroke="hsl(var(--accent))"
                  strokeWidth={3}
                  dot={{ fill: "hsl(var(--accent))", r: 4 }}
                />
              </LineChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Expense Breakdown */}
        <Card>
          <CardHeader>
            <CardTitle>Expense Breakdown</CardTitle>
            <CardDescription>This month's spending by category</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={250}>
              <PieChart>
                <Pie
                  data={expenseData}
                  cx="50%"
                  cy="50%"
                  innerRadius={60}
                  outerRadius={90}
                  paddingAngle={2}
                  dataKey="value"
                >
                  {expenseData.map((entry, index) => (
                    <Cell key={`cell-${index}`} fill={entry.color} />
                  ))}
                </Pie>
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(var(--popover))",
                    border: "1px solid hsl(var(--border))",
                    borderRadius: "var(--radius)",
                  }}
                />
              </PieChart>
            </ResponsiveContainer>
            <div className="mt-4 space-y-2">
              {expenseData.map((item) => (
                <div key={item.name} className="flex items-center justify-between text-sm">
                  <div className="flex items-center gap-2">
                    <div className="h-3 w-3 rounded-full" style={{ backgroundColor: item.color }} />
                    <span>{item.name}</span>
                  </div>
                  <span className="font-mono font-medium">${item.value}</span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>

        {/* Income vs Expense */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Income vs Expenses</CardTitle>
            <CardDescription>Last 6 months comparison</CardDescription>
          </CardHeader>
          <CardContent>
            <ResponsiveContainer width="100%" height={250}>
              <BarChart data={incomeExpenseData}>
                <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
                <XAxis dataKey="month" stroke="hsl(var(--muted-foreground))" />
                <YAxis stroke="hsl(var(--muted-foreground))" />
                <Tooltip
                  contentStyle={{
                    backgroundColor: "hsl(var(--popover))",
                    border: "1px solid hsl(var(--border))",
                    borderRadius: "var(--radius)",
                  }}
                />
                <Legend />
                <Bar dataKey="income" fill="hsl(var(--success))" radius={[4, 4, 0, 0]} />
                <Bar dataKey="expenses" fill="hsl(var(--destructive))" radius={[4, 4, 0, 0]} />
              </BarChart>
            </ResponsiveContainer>
          </CardContent>
        </Card>

        {/* Recent Transactions */}
        <Card className="lg:col-span-3">
          <CardHeader>
            <CardTitle>Recent Transactions</CardTitle>
            <CardDescription>Your latest financial activity</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              {recentTransactions.map((transaction) => (
                <div
                  key={transaction.id}
                  className="flex items-center justify-between p-3 rounded-lg border border-border hover:bg-muted/50 transition-colors"
                >
                  <div className="flex-1">
                    <p className="font-medium">{transaction.payee}</p>
                    <div className="flex items-center gap-3 mt-1">
                      <span className="text-xs text-muted-foreground">{transaction.date}</span>
                      <span className="text-xs px-2 py-1 rounded-full bg-muted text-muted-foreground">
                        {transaction.category}
                      </span>
                    </div>
                  </div>
                  <span
                    className={`text-lg font-mono font-semibold ${
                      transaction.amount > 0 ? "text-success" : "text-foreground"
                    }`}
                  >
                    {transaction.amount > 0 ? "+" : ""}${Math.abs(transaction.amount).toFixed(2)}
                  </span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
