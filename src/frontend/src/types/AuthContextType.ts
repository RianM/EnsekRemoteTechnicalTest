export type UserRole = 'anonymous' | 'manager';

export interface AuthContextType {
  role: UserRole;
  token: string | null;
  isManager: boolean;
  toggleRole: () => void;
  setToken: (token: string) => void;
}