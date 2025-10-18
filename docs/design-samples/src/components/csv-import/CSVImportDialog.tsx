import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { UploadStep } from "./UploadStep";
import { ColumnMappingStep } from "./ColumnMappingStep";
import { DuplicateDetectionStep } from "./DuplicateDetectionStep";
import { ConfirmationStep } from "./ConfirmationStep";

interface CSVImportDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

export type ImportStep = "upload" | "mapping" | "duplicates" | "confirmation";

export interface CSVFile {
  name: string;
  size: number;
  data: any[];
  columns: string[];
}

export interface ColumnMapping {
  date: string;
  amount: string;
  payee: string;
  memo: string;
}

export interface DuplicateMatch {
  existingTransaction: any;
  newTransaction: any;
  confidence: "exact" | "likely" | "possible";
  differences: string[];
  action: "skip" | "import" | null;
}

export function CSVImportDialog({ open, onOpenChange }: CSVImportDialogProps) {
  const [currentStep, setCurrentStep] = useState<ImportStep>("upload");
  const [csvFile, setCSVFile] = useState<CSVFile | null>(null);
  const [columnMapping, setColumnMapping] = useState<ColumnMapping | null>(null);
  const [duplicates, setDuplicates] = useState<DuplicateMatch[]>([]);
  const [finalCount, setFinalCount] = useState({ found: 0, skipped: 0, ready: 0, needsCategorization: 0 });

  const handleUploadComplete = (file: CSVFile) => {
    setCSVFile(file);
    setCurrentStep("mapping");
  };

  const handleMappingComplete = (mapping: ColumnMapping) => {
    setColumnMapping(mapping);
    // Mock duplicate detection
    const mockDuplicates: DuplicateMatch[] = [
      {
        existingTransaction: {
          date: "2025-01-15",
          payee: "Amazon",
          amount: -45.99,
          category: "Shopping",
        },
        newTransaction: {
          date: "2025-01-15",
          payee: "Amazon.com",
          amount: -45.99,
          category: "Uncategorized",
        },
        confidence: "exact",
        differences: ["payee"],
        action: null,
      },
      {
        existingTransaction: {
          date: "2025-01-12",
          payee: "Starbucks",
          amount: -5.45,
          category: "Dining",
        },
        newTransaction: {
          date: "2025-01-12",
          payee: "Starbucks Coffee",
          amount: -5.45,
          category: "Uncategorized",
        },
        confidence: "likely",
        differences: ["payee"],
        action: null,
      },
    ];
    setDuplicates(mockDuplicates);
    setCurrentStep("duplicates");
  };

  const handleDuplicatesComplete = (resolvedDuplicates: DuplicateMatch[]) => {
    const skipped = resolvedDuplicates.filter((d) => d.action === "skip").length;
    const found = csvFile?.data.length || 0;
    const ready = found - skipped;
    setFinalCount({ found, skipped, ready, needsCategorization: Math.floor(ready * 0.15) });
    setCurrentStep("confirmation");
  };

  const handleImportComplete = () => {
    // Reset state
    setCurrentStep("upload");
    setCSVFile(null);
    setColumnMapping(null);
    setDuplicates([]);
    onOpenChange(false);
  };

  const stepTitles = {
    upload: "Upload CSV",
    mapping: "Map Columns",
    duplicates: "Review Duplicates",
    confirmation: "Confirm Import",
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-4xl max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold">{stepTitles[currentStep]}</DialogTitle>
          <div className="flex items-center gap-2 mt-4">
            {(["upload", "mapping", "duplicates", "confirmation"] as ImportStep[]).map((step, index) => (
              <div key={step} className="flex items-center flex-1">
                <div
                  className={`h-2 flex-1 rounded-full ${
                    currentStep === step
                      ? "bg-accent"
                      : index <
                        (["upload", "mapping", "duplicates", "confirmation"] as ImportStep[]).indexOf(currentStep)
                      ? "bg-success"
                      : "bg-muted"
                  }`}
                />
              </div>
            ))}
          </div>
        </DialogHeader>

        {currentStep === "upload" && <UploadStep onComplete={handleUploadComplete} />}
        {currentStep === "mapping" && csvFile && (
          <ColumnMappingStep csvFile={csvFile} onComplete={handleMappingComplete} onBack={() => setCurrentStep("upload")} />
        )}
        {currentStep === "duplicates" && (
          <DuplicateDetectionStep
            duplicates={duplicates}
            onComplete={handleDuplicatesComplete}
            onBack={() => setCurrentStep("mapping")}
          />
        )}
        {currentStep === "confirmation" && (
          <ConfirmationStep finalCount={finalCount} onComplete={handleImportComplete} onBack={() => setCurrentStep("duplicates")} />
        )}
      </DialogContent>
    </Dialog>
  );
}
