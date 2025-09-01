import { useAuth } from '../../hooks/useAuth';

const Header = () => {
  const { isManager, toggleRole } = useAuth();

  return (
    <nav className="navbar navbar-dark bg-dark">
      <div className="container d-flex justify-content-between align-items-center">
        <span className="navbar-brand mb-0 h1">Energy Meter Management</span>
        <div className="d-flex align-items-center text-light">
          <label className="form-check-label me-3">Anonymous User</label>
          <div className="form-check form-switch m-0">
            <input
              className="form-check-input"
              type="checkbox"
              role="switch"
              data-testid="role-toggle"
              checked={isManager}
              onChange={toggleRole}
            />
          </div>
          <label className="form-check-label ms-3">Energy Company Manager</label>
        </div>
      </div>
    </nav>
  );
};

export default Header;
