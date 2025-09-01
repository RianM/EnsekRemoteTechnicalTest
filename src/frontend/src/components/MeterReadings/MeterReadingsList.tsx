import { useState, useEffect } from 'react';
import { meterReadingsService } from '../../services/meterReadingsService';
import { setAuthToken } from '../../services/apiClient';
import { useAuth } from '../../hooks/useAuth';
import type { MeterReadingDto } from '../../types';
import MeterReadingsTable from './MeterReadingsTable';
import CsvUpload from './CsvUpload';

const MeterReadingsList = () => {
  const { token, isManager } = useAuth();
  const [meterReadings, setMeterReadings] = useState<MeterReadingDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchMeterReadings = async () => {
    if (!token) return;
    
    setLoading(true);
    setError(null);
    
    try {
      setAuthToken(token);
      const data = await meterReadingsService.getAll();
      setMeterReadings(data);
    } catch (err) {
      setError('Failed to fetch meter readings');
      console.error('Error fetching meter readings:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchMeterReadings();
  }, [token]);

  const handleUploadComplete = () => {
    // Refresh the meter readings list after successful upload
    fetchMeterReadings();
  };

  if (error) {
    return <div className="alert alert-danger" role="alert" data-testid="error-message">{error}</div>;
  }

  return (
    <div>
      <div className="mb-3">
        <h2 className="h4 mb-1">Meter Readings</h2>
        <p className="text-muted mb-0" data-testid="section-description">
          View all meter readings in the system
          {isManager && ' and upload new readings via CSV'}
        </p>
      </div>
      
      {isManager && (
        <CsvUpload onUploadComplete={handleUploadComplete} />
      )}
      
      <MeterReadingsTable meterReadings={meterReadings} loading={loading} />
    </div>
  );
};

export default MeterReadingsList;
