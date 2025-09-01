import apiClient from './apiClient';
import type { AuthTokenResponse } from '../types';

export const authService = {
  getAnonymousToken: async (): Promise<AuthTokenResponse> => {
    const response = await apiClient.post('/auth/token/anonymous');
    return response.data;
  },

  getManagerToken: async (): Promise<AuthTokenResponse> => {
    const response = await apiClient.post('/auth/token/manager');
    return response.data;
  },
};
