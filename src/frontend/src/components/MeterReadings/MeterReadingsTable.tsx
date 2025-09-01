import type { MeterReadingDto } from '../../types';

interface MeterReadingsTableProps {
  meterReadings: MeterReadingDto[];
  loading: boolean;
}

const MeterReadingsTable = ({ meterReadings, loading }: MeterReadingsTableProps) => {
  if (loading) {
    return (
      <div className="d-flex justify-content-center my-4" data-testid="loading">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (meterReadings.length === 0) {
    return <div className="alert alert-info" data-testid="empty-state">No meter readings found</div>;
  }

  const formatDateTime = (dateString: string) => {
    return new Date(dateString).toLocaleString();
  };

  return (
    <div className="table-responsive">
      <table className="table table-striped table-hover align-middle" data-testid="data-table">
        <thead>
          <tr>
            <th>Account ID</th>
            <th>Reading Date/Time</th>
            <th>Meter Value</th>
          </tr>
        </thead>
        <tbody>
          {meterReadings.map((reading) => (
            <tr key={reading.accountId + reading.meterReadingDateTime}>
              <td>{reading.accountId}</td>
              <td>{formatDateTime(reading.meterReadingDateTime)}</td>
              <td>{reading.meterReadValue.toLocaleString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
};

export default MeterReadingsTable;
