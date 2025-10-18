import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { TrendingUp, AlertCircle, Repeat, Home, Smartphone, DollarSign } from "lucide-react";
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  ReferenceLine,
} from "recharts";

const cashFlowData = [
  { date: "Feb 1", balance: 8340, confidence: 100 },
  { date: "Feb 5", balance: 8100, confidence: 95 },
  { date: "Feb 10", balance: 9500, confidence: 90 },
  { date: "Feb 15", balance: 9300, confidence: 85 },
  { date: "Feb 20", balance: 9800, confidence: 80 },
  { date: "Feb 25", balance: 9600, confidence: 75 },
  { date: "Mar 1", balance: 10200, confidence: 70 },
  { date: "Mar 5", balance: 10000, confidence: 65 },
  { date: "Mar 10", balance: 10500, confidence: 60 },
  { date: "Mar 15", balance: 10300, confidence: 55 },
];

const recurringTransactions = [
  { id: 1, payee: "Rent", amount: -1500, frequency: "monthly", nextDate: "2025-03-01", icon: Home, color: "text-chart-4" },
  { id: 2, payee: "Salary", amount: 5200, frequency: "monthly", nextDate: "2025-02-03", icon: DollarSign, color: "text-success" },
  { id: 3, payee: "Netflix", amount: -15.99, frequency: "monthly", nextDate: "2025-02-15", icon: Smartphone, color: "text-accent" },
  { id: 4, payee: "Electric Bill", amount: -120, frequency: "monthly", nextDate: "2025-02-20", icon: Repeat, color: "text-chart-5" },
];

export default function CashFlow() {
  return (
    <div className="p-6 space-y-6 animate-fade-in">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-foreground">Cash Flow Timeline</h1>
          <p className="text-muted-foreground mt-1">Predicted balance and recurring transactions</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline">30 Days</Button>
          <Button variant="outline">60 Days</Button>
          <Button variant="outline">90 Days</Button>
        </div>
      </div>

      {/* Alert Banner */}
      <Card className="border-accent bg-accent/5">
        <CardContent className="p-4">
          <div className="flex items-center gap-3">
            <TrendingUp className="h-5 w-5 text-accent" />
            <div>
              <p className="font-medium text-foreground">On track for positive growth</p>
              <p className="text-sm text-muted-foreground">
                Your predicted balance shows a +12% increase over the next 30 days
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Main Chart */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <TrendingUp className="h-5 w-5 text-accent" />
            Balance Prediction
          </CardTitle>
          <CardDescription>30-day forecast with confidence indicators</CardDescription>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={400}>
            <LineChart data={cashFlowData}>
              <defs>
                <linearGradient id="confidenceGradient" x1="0" y1="0" x2="0" y2="1">
                  <stop offset="0%" stopColor="hsl(var(--accent))" stopOpacity={0.3} />
                  <stop offset="100%" stopColor="hsl(var(--accent))" stopOpacity={0.05} />
                </linearGradient>
              </defs>
              <CartesianGrid strokeDasharray="3 3" stroke="hsl(var(--border))" />
              <XAxis dataKey="date" stroke="hsl(var(--muted-foreground))" />
              <YAxis stroke="hsl(var(--muted-foreground))" />
              <Tooltip
                contentStyle={{
                  backgroundColor: "hsl(var(--popover))",
                  border: "1px solid hsl(var(--border))",
                  borderRadius: "var(--radius)",
                }}
                formatter={(value: number, name: string) => {
                  if (name === "balance") return [`$${value.toLocaleString()}`, "Balance"];
                  return [value, name];
                }}
              />
              <ReferenceLine y={0} stroke="hsl(var(--destructive))" strokeDasharray="3 3" />
              <Line
                type="monotone"
                dataKey="balance"
                stroke="hsl(var(--accent))"
                strokeWidth={3}
                dot={{ fill: "hsl(var(--accent))", r: 5 }}
                fill="url(#confidenceGradient)"
              />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {/* Recurring Transactions */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Repeat className="h-5 w-5 text-accent" />
            Recurring Transactions
          </CardTitle>
          <CardDescription>Expected payments and income</CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {recurringTransactions.map((transaction) => {
              const IconComponent = transaction.icon;
              return (
                <div
                  key={transaction.id}
                  className="flex items-center justify-between p-4 rounded-lg border border-border hover:bg-muted/50 transition-colors"
                >
                  <div className="flex items-center gap-4">
                    <div className={`p-2 rounded-lg bg-muted ${transaction.color}`}>
                      <IconComponent className="h-5 w-5" />
                    </div>
                    <div>
                      <p className="font-medium">{transaction.payee}</p>
                      <div className="flex items-center gap-2 mt-1">
                        <Badge variant="outline" className="text-xs">
                          {transaction.frequency}
                        </Badge>
                        <span className="text-xs text-muted-foreground">Next: {transaction.nextDate}</span>
                      </div>
                    </div>
                  </div>
                  <span
                    className={`text-lg font-mono font-semibold ${
                      transaction.amount > 0 ? "text-success" : "text-foreground"
                    }`}
                  >
                    {transaction.amount > 0 ? "+" : ""}${Math.abs(transaction.amount).toLocaleString()}
                  </span>
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
