import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Database, Tag, FileText, Download } from "lucide-react";

const hledgerPreview = `2025-02-01 * Whole Foods
    expenses:groceries           $85.23
    liabilities:credit-card

2025-02-03 * Salary Deposit
    assets:checking           $5200.00
    income:salary

2025-02-04 * Electric Company
    expenses:utilities          $120.45
    assets:checking`;

export default function Settings() {
  return (
    <div className="p-6 space-y-6 animate-fade-in">
      <div>
        <h1 className="text-3xl font-bold text-foreground">Settings</h1>
        <p className="text-muted-foreground mt-1">Manage your accounts, categories, and export options</p>
      </div>

      <Tabs defaultValue="accounts" className="space-y-6">
        <TabsList>
          <TabsTrigger value="accounts" className="gap-2">
            <Database className="h-4 w-4" />
            Accounts
          </TabsTrigger>
          <TabsTrigger value="categories" className="gap-2">
            <Tag className="h-4 w-4" />
            Categories
          </TabsTrigger>
          <TabsTrigger value="export" className="gap-2">
            <FileText className="h-4 w-4" />
            Export
          </TabsTrigger>
        </TabsList>

        <TabsContent value="accounts" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Bank Accounts</CardTitle>
              <CardDescription>Add and manage your financial accounts</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="account-name">Account Name</Label>
                <Input id="account-name" placeholder="e.g., Chase Checking" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="account-type">Account Type</Label>
                <Input id="account-type" placeholder="e.g., Checking, Savings, Credit Card" />
              </div>
              <div className="space-y-2">
                <Label htmlFor="initial-balance">Initial Balance</Label>
                <Input id="initial-balance" type="number" placeholder="0.00" className="font-mono" />
              </div>
              <Button className="bg-accent hover:bg-accent/90">Add Account</Button>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="categories" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Categories</CardTitle>
              <CardDescription>Customize your transaction categories</CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="category-name">Category Name</Label>
                <Input id="category-name" placeholder="e.g., Groceries, Transport" />
              </div>
              <Button className="bg-accent hover:bg-accent/90">Add Category</Button>

              <div className="mt-6 space-y-2">
                <h3 className="font-medium">Existing Categories</h3>
                <div className="flex flex-wrap gap-2">
                  {["Groceries", "Dining", "Transport", "Entertainment", "Utilities", "Income"].map(
                    (category) => (
                      <div
                        key={category}
                        className="px-3 py-2 rounded-lg border border-border bg-muted/50 text-sm"
                      >
                        {category}
                      </div>
                    )
                  )}
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="export" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>.hledger File Export</CardTitle>
              <CardDescription>
                Export your transactions in plain text accounting format
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label>File Preview</Label>
                <Textarea
                  value={hledgerPreview}
                  readOnly
                  className="font-mono text-sm h-[300px] bg-muted"
                />
              </div>
              <div className="flex gap-2">
                <Button className="bg-accent hover:bg-accent/90">
                  <Download className="mr-2 h-4 w-4" />
                  Download .hledger File
                </Button>
                <Button variant="outline">Copy to Clipboard</Button>
              </div>
              <p className="text-sm text-muted-foreground">
                Your financial data remains local and private. The .hledger format is compatible with
                plain text accounting tools.
              </p>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}
