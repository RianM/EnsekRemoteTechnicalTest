import { useState, useEffect, type ReactNode } from 'react';
import type { UserRole, AuthContextType } from '../types';
import { authService } from '../services/authService';
import { AuthContext } from '../context/authContext';

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider = ({ children }: AuthProviderProps) => {
  const [role, setRole] = useState<UserRole>('anonymous');
  const [token, setTokenState] = useState<string | null>(null);

  const isManager = role === 'manager';

  const setToken = (newToken: string) => {
    setTokenState(newToken);
  };

  const toggleRole = async () => {
    const newRole: UserRole = role === 'anonymous' ? 'manager' : 'anonymous';
    setRole(newRole);
    
    try {
      let tokenResponse;
      if (newRole === 'manager') {
        tokenResponse = await authService.getManagerToken();
      } else {
        tokenResponse = await authService.getAnonymousToken();
      }
      setToken(tokenResponse.token);
    } catch (error) {
      console.error('Failed to get token:', error);
      setTokenState(null);
    }
  };

  // Initialize with anonymous token on mount
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const tokenResponse = await authService.getAnonymousToken();
        setToken(tokenResponse.token);
      } catch (error) {
        console.error('Failed to initialize auth:', error);
      }
    };

    initializeAuth();
  }, []);

  const value: AuthContextType = {
    role,
    token,
    isManager,
    toggleRole,
    setToken,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};