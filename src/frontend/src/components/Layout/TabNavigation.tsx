import { Link, useLocation } from 'react-router-dom';

const TabNavigation = () => {
  const location = useLocation();
  const currentPath = location.pathname;

  return (
    <nav className="bg-light border-bottom">
      <div className="container">
        <ul className="nav nav-tabs">
          <li className="nav-item">
            <Link
              to="/accounts"
              className={`nav-link ${currentPath === '/accounts' || currentPath === '/' ? 'active' : ''}`}
            >
              Accounts
            </Link>
          </li>
          <li className="nav-item">
            <Link
              to="/meter-readings"
              className={`nav-link ${currentPath === '/meter-readings' ? 'active' : ''}`}
            >
              Meter Readings
            </Link>
          </li>
        </ul>
      </div>
    </nav>
  );
};

export default TabNavigation;
