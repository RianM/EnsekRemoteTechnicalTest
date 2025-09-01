export interface MeterReadingUploadErrorDto {
  row: number;
  accountId?: number;
  error: string;
  rawData: string;
}