import { useState, useEffect } from "react";
import { CheckCircle2, AlertTriangle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { CSVFile, ColumnMapping } from "./CSVImportDialog";

interface ColumnMappingStepProps {
  csvFile: CSVFile;
  onComplete: (mapping: ColumnMapping) => void;
  onBack: () => void;
}

export function ColumnMappingStep({ csvFile, onComplete, onBack }: ColumnMappingStepProps) {
  const [mapping, setMapping] = useState<ColumnMapping>({
    date: "",
    amount: "",
    payee: "",
    memo: "",
  });

  const [confidence, setConfidence] = useState<Record<keyof ColumnMapping, number>>({
    date: 0,
    amount: 0,
    payee: 0,
    memo: 0,
  });

  // Auto-detect columns
  useEffect(() => {
    const autoMapping: ColumnMapping = { date: "", amount: "", payee: "", memo: "" };
    const autoConfidence: Record<keyof ColumnMapping, number> = { date: 0, amount: 0, payee: 0, memo: 0 };

    csvFile.columns.forEach((col) => {
      const lower = col.toLowerCase();
      if (lower.includes("date") && !autoMapping.date) {
        autoMapping.date = col;
        autoConfidence.date = 95;
      } else if ((lower.includes("amount") || lower.includes("price")) && !autoMapping.amount) {
        autoMapping.amount = col;
        autoConfidence.amount = 90;
      } else if ((lower.includes("payee") || lower.includes("merchant") || lower.includes("description")) && !autoMapping.payee) {
        autoMapping.payee = col;
        autoConfidence.payee = 85;
      } else if ((lower.includes("memo") || lower.includes("note")) && !autoMapping.memo) {
        autoMapping.memo = col;
        autoConfidence.memo = 80;
      }
    });

    setMapping(autoMapping);
    setConfidence(autoConfidence);
  }, [csvFile.columns]);

  const handleMappingChange = (field: keyof ColumnMapping, value: string) => {
    setMapping((prev) => ({ ...prev, [field]: value }));
    setConfidence((prev) => ({ ...prev, [field]: 100 }));
  };

  const isValid = mapping.date && mapping.amount && mapping.payee;

  const getConfidenceIcon = (conf: number) => {
    if (conf >= 90) return <CheckCircle2 className="h-5 w-5 text-success" />;
    if (conf >= 70) return <AlertTriangle className="h-5 w-5 text-yellow-500" />;
    return <AlertTriangle className="h-5 w-5 text-destructive" />;
  };

  return (
    <div className="space-y-6 py-6">
      <div className="space-y-4">
        {(["date", "amount", "payee", "memo"] as const).map((field) => (
          <div key={field} className="grid grid-cols-[1fr_2fr_auto] gap-4 items-center">
            <label className="font-medium capitalize">{field} {field !== "memo" && <span className="text-destructive">*</span>}</label>
            <Select value={mapping[field]} onValueChange={(value) => handleMappingChange(field, value)}>
              <SelectTrigger>
                <SelectValue placeholder={`Select ${field} column`} />
              </SelectTrigger>
              <SelectContent>
                {csvFile.columns.map((col) => (
                  <SelectItem key={col} value={col}>
                    {col}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {mapping[field] && getConfidenceIcon(confidence[field])}
          </div>
        ))}
      </div>

      <div className="bg-muted rounded-lg p-4">
        <h3 className="font-semibold mb-2">Preview (First 5 Rows)</h3>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b">
                <th className="text-left py-2 px-2 bg-accent/10">Date</th>
                <th className="text-left py-2 px-2 bg-accent/10">Amount</th>
                <th className="text-left py-2 px-2 bg-accent/10">Payee</th>
                <th className="text-left py-2 px-2 bg-accent/10">Memo</th>
              </tr>
            </thead>
            <tbody>
              {csvFile.data.slice(0, 5).map((row, index) => (
                <tr key={index} className="border-b">
                  <td className="py-2 px-2">{mapping.date ? row[mapping.date] : "-"}</td>
                  <td className="py-2 px-2">{mapping.amount ? row[mapping.amount] : "-"}</td>
                  <td className="py-2 px-2">{mapping.payee ? row[mapping.payee] : "-"}</td>
                  <td className="py-2 px-2">{mapping.memo ? row[mapping.memo] : "-"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      <div className="flex justify-between">
        <Button variant="outline" onClick={onBack}>
          Back
        </Button>
        <Button onClick={() => onComplete(mapping)} disabled={!isValid} size="lg">
          Continue to Duplicate Check
        </Button>
      </div>
    </div>
  );
}
