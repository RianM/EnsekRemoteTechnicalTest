import type { AuthTokenResponse } from '../types';

export const authService = {
  getAnonymousToken: async (): Promise<AuthTokenResponse> => {
    // Stubbed successful response (backend auth not implemented yet)
    return Promise.resolve({
      token: `stub-anonymous-token-${Date.now()}`,
      role: 'anonymous',
      message: 'Authenticated as anonymous (stub)'
    });
  },

  getManagerToken: async (): Promise<AuthTokenResponse> => {
    // Stubbed successful response (backend auth not implemented yet)
    return Promise.resolve({
      token: `stub-manager-token-${Date.now()}`,
      role: 'manager',
      message: 'Authenticated as manager (stub)'
    });
  },
};
