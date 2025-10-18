import { CheckCircle2, AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useToast } from "@/hooks/use-toast";

interface ConfirmationStepProps {
  finalCount: {
    found: number;
    skipped: number;
    ready: number;
    needsCategorization: number;
  };
  onComplete: () => void;
  onBack: () => void;
}

export function ConfirmationStep({ finalCount, onComplete, onBack }: ConfirmationStepProps) {
  const { toast } = useToast();

  const handleImport = () => {
    // Simulate API call
    setTimeout(() => {
      toast({
        title: "Import Successful",
        description: `Imported ${finalCount.ready} transactions successfully`,
      });
      onComplete();
    }, 1000);
  };

  return (
    <div className="space-y-6 py-6">
      <div className="text-center py-8">
        <CheckCircle2 className="h-20 w-20 mx-auto mb-6 text-success" />
        <h2 className="text-2xl font-bold mb-2">Ready to Import</h2>
        <p className="text-muted-foreground">Review the summary below before importing</p>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="bg-muted rounded-lg p-6 text-center">
          <p className="text-3xl font-bold text-primary">{finalCount.found}</p>
          <p className="text-sm text-muted-foreground mt-1">Transactions Found</p>
        </div>
        <div className="bg-muted rounded-lg p-6 text-center">
          <p className="text-3xl font-bold text-yellow-600">{finalCount.skipped}</p>
          <p className="text-sm text-muted-foreground mt-1">Duplicates Skipped</p>
        </div>
        <div className="bg-muted rounded-lg p-6 text-center">
          <p className="text-3xl font-bold text-success">{finalCount.ready}</p>
          <p className="text-sm text-muted-foreground mt-1">Ready to Import</p>
        </div>
        <div className="bg-muted rounded-lg p-6 text-center">
          <p className="text-3xl font-bold text-accent">{finalCount.needsCategorization}</p>
          <p className="text-sm text-muted-foreground mt-1">Need Categorization</p>
        </div>
      </div>

      <div className="bg-accent/10 border border-accent/20 text-accent-foreground px-4 py-3 rounded-lg flex items-start gap-3">
        <AlertCircle className="h-5 w-5 flex-shrink-0 mt-0.5" />
        <div>
          <p className="font-semibold">Ledgerly learns from your corrections</p>
          <p className="text-sm mt-1">
            Review the uncategorized transactions after import. Your categorization choices will improve future imports.
          </p>
        </div>
      </div>

      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Button onClick={handleImport} size="lg" className="bg-success hover:bg-success/90">
          Import {finalCount.ready} Transactions
        </Button>
      </div>
    </div>
  );
}
