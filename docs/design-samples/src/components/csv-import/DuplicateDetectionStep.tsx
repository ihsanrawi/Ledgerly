import { useState } from "react";
import { AlertCircle, CheckCircle2, HelpCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { DuplicateMatch } from "./CSVImportDialog";

interface DuplicateDetectionStepProps {
  duplicates: DuplicateMatch[];
  onComplete: (resolvedDuplicates: DuplicateMatch[]) => void;
  onBack: () => void;
}

export function DuplicateDetectionStep({ duplicates, onComplete, onBack }: DuplicateDetectionStepProps) {
  const [resolvedDuplicates, setResolvedDuplicates] = useState<DuplicateMatch[]>(duplicates);

  const handleAction = (index: number, action: "skip" | "import") => {
    setResolvedDuplicates((prev) =>
      prev.map((dup, i) => (i === index ? { ...dup, action } : dup))
    );
  };

  const handleSkipAll = () => {
    setResolvedDuplicates((prev) =>
      prev.map((dup) => (dup.action === null ? { ...dup, action: "skip" } : dup))
    );
  };

  const getConfidenceBadge = (confidence: DuplicateMatch["confidence"]) => {
    const variants = {
      exact: { color: "bg-success text-white", label: "Exact Match", icon: CheckCircle2 },
      likely: { color: "bg-yellow-500 text-white", label: "Likely Match", icon: AlertCircle },
      possible: { color: "bg-muted text-muted-foreground", label: "Possible Match", icon: HelpCircle },
    };
    const variant = variants[confidence];
    const Icon = variant.icon;

    return (
      <Badge className={`${variant.color} gap-1`}>
        <Icon className="h-3 w-3" />
        {variant.label}
      </Badge>
    );
  };

  const skippedCount = resolvedDuplicates.filter((d) => d.action === "skip").length;

  return (
    <div className="space-y-6 py-6">
      {duplicates.length === 0 ? (
        <div className="text-center py-12">
          <CheckCircle2 className="h-16 w-16 mx-auto mb-4 text-success" />
          <h3 className="text-xl font-semibold mb-2">No Duplicates Found</h3>
          <p className="text-muted-foreground">All transactions are unique and ready to import</p>
        </div>
      ) : (
        <>
          <div className="bg-yellow-500/10 border border-yellow-500/20 text-yellow-800 dark:text-yellow-200 px-4 py-3 rounded-lg flex items-start gap-3">
            <AlertCircle className="h-5 w-5 flex-shrink-0 mt-0.5" />
            <div>
              <p className="font-semibold">{duplicates.length} potential duplicates detected</p>
              <p className="text-sm mt-1">Review each match below and choose an action</p>
            </div>
          </div>

          <div className="space-y-4">
            {resolvedDuplicates.map((dup, index) => (
              <div key={index} className="border rounded-lg p-4 space-y-3">
                <div className="flex items-center justify-between">
                  {getConfidenceBadge(dup.confidence)}
                  <div className="flex gap-2">
                    <Button
                      size="sm"
                      variant={dup.action === "skip" ? "default" : "outline"}
                      onClick={() => handleAction(index, "skip")}
                    >
                      Skip This
                    </Button>
                    <Button
                      size="sm"
                      variant={dup.action === "import" ? "default" : "outline"}
                      onClick={() => handleAction(index, "import")}
                    >
                      Import Anyway
                    </Button>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <p className="text-sm font-semibold text-muted-foreground">Existing Transaction</p>
                    <div className="bg-muted rounded p-3 space-y-1 text-sm">
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Date:</span>
                        <span>{dup.existingTransaction.date}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Payee:</span>
                        <span>{dup.existingTransaction.payee}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Amount:</span>
                        <span className="font-mono">${Math.abs(dup.existingTransaction.amount).toFixed(2)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Category:</span>
                        <Badge variant="outline">{dup.existingTransaction.category}</Badge>
                      </div>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <p className="text-sm font-semibold text-muted-foreground">New Transaction</p>
                    <div className="bg-muted rounded p-3 space-y-1 text-sm">
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Date:</span>
                        <span>{dup.newTransaction.date}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Payee:</span>
                        <span
                          className={dup.differences.includes("payee") ? "bg-yellow-200 dark:bg-yellow-900/50 px-1" : ""}
                        >
                          {dup.newTransaction.payee}
                        </span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Amount:</span>
                        <span className="font-mono">${Math.abs(dup.newTransaction.amount).toFixed(2)}</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Category:</span>
                        <Badge variant="outline">{dup.newTransaction.category}</Badge>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            ))}
          </div>

          <div className="flex justify-between items-center">
            <Button variant="outline" onClick={handleSkipAll}>
              Skip All Remaining
            </Button>
            <p className="text-sm text-muted-foreground">
              {skippedCount} of {duplicates.length} marked to skip
            </p>
          </div>
        </>
      )}

      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Button onClick={() => onComplete(resolvedDuplicates)} size="lg">
          Continue to Confirmation
        </Button>
      </div>
    </div>
  );
}
