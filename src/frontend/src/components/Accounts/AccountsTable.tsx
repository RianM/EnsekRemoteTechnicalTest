import type { AccountDto } from '../../types';

interface AccountsTableProps {
  accounts: AccountDto[];
  loading: boolean;
}

const AccountsTable = ({ accounts, loading }: AccountsTableProps) => {
  if (loading) {
    return (
      <div className="d-flex justify-content-center my-4" data-testid="loading">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (accounts.length === 0) {
    return <div className="alert alert-info" data-testid="empty-state">No accounts found</div>;
  }

  return (
    <div className="table-responsive">
      <table className="table table-striped table-hover align-middle" data-testid="data-table">
        <thead>
          <tr>
            <th>Account ID</th>
            <th>First Name</th>
            <th>Last Name</th>
          </tr>
        </thead>
        <tbody>
          {accounts.map((account) => (
            <tr key={account.accountId}>
              <td>{account.accountId}</td>
              <td>{account.firstName}</td>
              <td>{account.lastName}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default AccountsTable;
