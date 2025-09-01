import { useState, useEffect } from 'react';
import { accountsService } from '../../services/accountsService';
import { setAuthToken } from '../../services/apiClient';
import { useAuth } from '../../hooks/useAuth';
import type { AccountDto } from '../../types';
import AccountsTable from './AccountsTable';

const AccountsList = () => {
  const { token } = useAuth();
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAccounts = async () => {
      if (!token) return;
      
      setLoading(true);
      setError(null);
      
      try {
        setAuthToken(token);
        const data = await accountsService.getAll();
        setAccounts(data);
      } catch (err) {
        setError('Failed to fetch accounts');
        console.error('Error fetching accounts:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchAccounts();
  }, [token]);

  if (error) {
    return <div className="alert alert-danger" role="alert" data-testid="error-message">{error}</div>;
  }

  return (
    <div>
      <div className="mb-3">
        <h2 className="h4 mb-1">Accounts</h2>
        <p className="text-muted mb-0" data-testid="section-description">View all energy meter accounts in the system</p>
      </div>
      <AccountsTable accounts={accounts} loading={loading} />
    </div>
  );
};

export default AccountsList;
