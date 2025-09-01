import { useAuth } from '../../hooks/useAuth';

const Header = () => {
  const { isManager, toggleRole } = useAuth();

  return (
    <nav className="navbar navbar-dark bg-dark">
      <div className="container d-flex justify-content-between align-items-center">
        <span className="navbar-brand mb-0 h1">Energy Meter Management</span>
        <div className="form-check form-switch text-light m-0">
          <label className="form-check-label me-2">Anonymous User</label>
          <input
            className="form-check-input"
            type="checkbox"
            role="switch"
            data-testid="role-toggle"
            checked={isManager}
            onChange={toggleRole}
          />
          <label className="form-check-label ms-2">Energy Company Manager</label>
        </div>
      </div>
    </nav>
  );
};

export default Header;
