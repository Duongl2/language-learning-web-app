export interface CsvPreview {
  validCount: number;
  invalidCount: number;
  rows: { line: number; front: string; back: string; error: string | null }[];
}
