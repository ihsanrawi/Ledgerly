import { useState } from "react";
import { Home, List, TrendingUp, BarChart3, Settings, Upload, Plus } from "lucide-react";
import { NavLink } from "react-router-dom";
import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { CSVImportDialog } from "@/components/csv-import/CSVImportDialog";

const navItems = [
  { name: "Dashboard", path: "/", icon: Home },
  { name: "Transactions", path: "/transactions", icon: List },
  { name: "Cash Flow", path: "/cash-flow", icon: TrendingUp },
  { name: "Reports", path: "/reports", icon: BarChart3 },
  { name: "Settings", path: "/settings", icon: Settings },
];

export function Sidebar() {
  const [importDialogOpen, setImportDialogOpen] = useState(false);

  return (
    <>
      <CSVImportDialog open={importDialogOpen} onOpenChange={setImportDialogOpen} />
      <aside className="w-64 bg-sidebar border-r border-sidebar-border flex flex-col h-screen sticky top-0">
      <div className="p-6 border-b border-sidebar-border">
        <h1 className="text-2xl font-bold text-sidebar-foreground">Ledgerly</h1>
        <p className="text-xs text-muted-foreground mt-1">Personal Finance Manager</p>
      </div>

      <nav className="flex-1 p-4 space-y-1">
        {navItems.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            end={item.path === "/"}
            className={({ isActive }) =>
              cn(
                "flex items-center gap-3 px-4 py-3 rounded-lg transition-all duration-200",
                "hover:bg-sidebar-accent text-sidebar-foreground",
                isActive && "bg-sidebar-accent border-l-4 border-accent font-medium"
              )
            }
          >
            <item.icon className="h-5 w-5" />
            <span>{item.name}</span>
          </NavLink>
        ))}
      </nav>

      <div className="p-4 space-y-2 border-t border-sidebar-border">
        <Button 
          className="w-full bg-accent hover:bg-accent/90 text-accent-foreground" 
          size="lg"
          onClick={() => setImportDialogOpen(true)}
        >
          <Upload className="mr-2 h-5 w-5" />
          Import CSV
        </Button>
        <Button variant="outline" className="w-full" size="lg">
          <Plus className="mr-2 h-5 w-5" />
          Add Transaction
        </Button>
      </div>
    </aside>
    </>
  );
}
