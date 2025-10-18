import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Search, Filter, Edit, Trash2 } from "lucide-react";

const transactions = [
  { id: 1, date: "2025-02-01", payee: "Whole Foods", amount: -85.23, category: "Groceries", account: "Checking" },
  { id: 2, date: "2025-02-01", payee: "Shell Gas", amount: -45.00, category: "Transport", account: "Credit Card" },
  { id: 3, date: "2025-02-02", payee: "Netflix", amount: -15.99, category: "Entertainment", account: "Credit Card" },
  { id: 4, date: "2025-02-03", payee: "Salary Deposit", amount: 5200.00, category: "Income", account: "Checking" },
  { id: 5, date: "2025-02-04", payee: "Electric Company", amount: -120.45, category: "Utilities", account: "Checking" },
  { id: 6, date: "2025-02-05", payee: "Target", amount: -67.89, category: "Groceries", account: "Credit Card" },
  { id: 7, date: "2025-02-06", payee: "Starbucks", amount: -12.50, category: "Dining", account: "Credit Card" },
  { id: 8, date: "2025-02-07", payee: "Gas Station", amount: -52.00, category: "Transport", account: "Credit Card" },
];

const getCategoryColor = (category: string) => {
  const colors: Record<string, string> = {
    Income: "bg-success/10 text-success border-success/20",
    Groceries: "bg-chart-1/10 text-chart-1 border-chart-1/20",
    Transport: "bg-chart-2/10 text-chart-2 border-chart-2/20",
    Entertainment: "bg-chart-4/10 text-chart-4 border-chart-4/20",
    Utilities: "bg-chart-5/10 text-chart-5 border-chart-5/20",
    Dining: "bg-accent/10 text-accent border-accent/20",
  };
  return colors[category] || "bg-muted text-muted-foreground";
};

export default function Transactions() {
  return (
    <div className="p-6 space-y-6 animate-fade-in">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Transactions</h1>
        <p className="text-muted-foreground mt-1">View and manage all your transactions</p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex flex-col md:flex-row gap-4 items-start md:items-center justify-between">
            <div>
              <CardTitle>All Transactions</CardTitle>
              <CardDescription>Filter and search through your transaction history</CardDescription>
            </div>
            <div className="flex gap-2">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                <Input placeholder="Search transactions..." className="pl-10 w-[250px]" />
              </div>
              <Button variant="outline" size="icon">
                <Filter className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="rounded-lg border border-border overflow-hidden">
            <table className="w-full">
              <thead className="bg-muted/50">
                <tr>
                  <th className="text-left p-4 font-medium text-sm">Date</th>
                  <th className="text-left p-4 font-medium text-sm">Payee</th>
                  <th className="text-left p-4 font-medium text-sm">Category</th>
                  <th className="text-left p-4 font-medium text-sm">Account</th>
                  <th className="text-right p-4 font-medium text-sm">Amount</th>
                  <th className="text-right p-4 font-medium text-sm">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {transactions.map((transaction) => (
                  <tr key={transaction.id} className="hover:bg-muted/30 transition-colors">
                    <td className="p-4 text-sm font-mono">{transaction.date}</td>
                    <td className="p-4 text-sm font-medium">{transaction.payee}</td>
                    <td className="p-4">
                      <Badge variant="outline" className={getCategoryColor(transaction.category)}>
                        {transaction.category}
                      </Badge>
                    </td>
                    <td className="p-4 text-sm text-muted-foreground">{transaction.account}</td>
                    <td className={`p-4 text-sm font-mono font-semibold text-right ${
                      transaction.amount > 0 ? "text-success" : "text-foreground"
                    }`}>
                      {transaction.amount > 0 ? "+" : ""}${Math.abs(transaction.amount).toFixed(2)}
                    </td>
                    <td className="p-4">
                      <div className="flex gap-2 justify-end">
                        <Button variant="ghost" size="icon" className="h-8 w-8">
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="icon" className="h-8 w-8 text-destructive">
                          <Trash2 className="h-4 w-4" />
                        </Button>
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
