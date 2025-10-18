import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Download, FileText, TrendingUp, TrendingDown } from "lucide-react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";

const categoryData = [
  { category: "Groceries", amount: 450, percentage: 28.5, count: 12, change: -5.2 },
  { category: "Dining", amount: 230, percentage: 14.6, count: 8, change: 12.3 },
  { category: "Transport", amount: 180, percentage: 11.4, count: 6, change: -2.1 },
  { category: "Entertainment", amount: 150, percentage: 9.5, count: 5, change: 8.7 },
  { category: "Utilities", amount: 120, percentage: 7.6, count: 3, change: 1.5 },
];

export default function Reports() {
  return (
    <div className="p-6 space-y-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Category Reports</h1>
          <p className="text-muted-foreground mt-1">Analyze your spending patterns</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">
            <FileText className="mr-2 h-4 w-4" />
            Export PDF
          </Button>
          <Button variant="outline">
            <Download className="mr-2 h-4 w-4" />
            Export CSV
          </Button>
        </div>
      </div>

      {/* Category Bar Chart */}
      <Card>
        <CardHeader>
          <CardTitle>Spending by Category</CardTitle>
          <CardDescription>This month compared to last month</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={350}>
            <BarChart data={categoryData} layout="vertical">
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
              <XAxis type="number" stroke="hsl(var(--muted-foreground))" />
              <YAxis dataKey="category" type="category" stroke="hsl(var(--muted-foreground))" width={120} />
              <Tooltip
                contentStyle={{
                  backgroundColor: "hsl(var(--popover))",
                  border: "1px solid hsl(var(--border))",
                  borderRadius: "var(--radius)",
                }}
              />
              <Bar dataKey="amount" fill="hsl(var(--accent))" radius={[0, 4, 4, 0]} />
            </BarChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Category Breakdown Table */}
      <Card>
        <CardHeader>
          <CardTitle>Detailed Breakdown</CardTitle>
          <CardDescription>Transaction count and trend analysis</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border border-border overflow-hidden">
            <table className="w-full">
              <thead className="bg-muted/50">
                <tr>
                  <th className="text-left p-4 font-medium text-sm">Category</th>
                  <th className="text-right p-4 font-medium text-sm">Amount</th>
                  <th className="text-right p-4 font-medium text-sm">% of Total</th>
                  <th className="text-right p-4 font-medium text-sm">Transactions</th>
                  <th className="text-right p-4 font-medium text-sm">vs Last Month</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {categoryData.map((item) => (
                  <tr key={item.category} className="hover:bg-muted/30 transition-colors">
                    <td className="p-4 font-medium">{item.category}</td>
                    <td className="p-4 text-right font-mono font-semibold">
                      ${item.amount.toLocaleString()}
                    </td>
                    <td className="p-4 text-right text-muted-foreground">
                      {item.percentage.toFixed(1)}%
                    </td>
                    <td className="p-4 text-right text-muted-foreground">{item.count}</td>
                    <td className="p-4 text-right">
                      <div className={`flex items-center justify-end gap-1 ${
                        item.change < 0 ? "text-success" : "text-destructive"
                      }`}>
                        {item.change < 0 ? (
                          <TrendingDown className="h-4 w-4" />
                        ) : (
                          <TrendingUp className="h-4 w-4" />
                        )}
                        <span className="font-medium">{Math.abs(item.change).toFixed(1)}%</span>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
