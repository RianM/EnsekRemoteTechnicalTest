import apiClient from './apiClient';
import type { MeterReadingDto, MeterReadingUploadResultDto } from '../types';

export const meterReadingsService = {
  getAll: async (): Promise<MeterReadingDto[]> => {
    const response = await apiClient.get('/meterreadings');
    return response.data;
  },

  uploadCsv: async (file: File): Promise<MeterReadingUploadResultDto> => {
    const formData = new FormData();
    formData.append('csvFile', file);
    
    const response = await apiClient.post('/meterreadings/meter-reading-uploads', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },
};