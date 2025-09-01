import apiClient from './apiClient';
import type { AccountDto } from '../types';

export const accountsService = {
  getAll: async (): Promise<AccountDto[]> => {
    const response = await apiClient.get('/accounts');
    return response.data;
  }
};