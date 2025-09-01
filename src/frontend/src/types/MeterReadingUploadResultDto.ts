import type { MeterReadingDto } from './MeterReadingDto';
import type { MeterReadingUploadErrorDto } from './MeterReadingUploadErrorDto';

export interface MeterReadingUploadResultDto {
  totalProcessed: number;
  successful: number;
  failed: number;
  successfulReadings: MeterReadingDto[];
  errors: MeterReadingUploadErrorDto[];
}