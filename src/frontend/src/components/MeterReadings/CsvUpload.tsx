import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { meterReadingsService } from '../../services/meterReadingsService';
import { setAuthToken } from '../../services/apiClient';
import { useAuth } from '../../hooks/useAuth';
import type { MeterReadingUploadResultDto } from '../../types';

interface CsvUploadFormData {
  csvFile: FileList;
}

interface CsvUploadProps {
  onUploadComplete: () => void;
}

const CsvUpload = ({ onUploadComplete }: CsvUploadProps) => {
  const { token, isManager } = useAuth();
  const [uploading, setUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState<MeterReadingUploadResultDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors }
  } = useForm<CsvUploadFormData>();

  if (!isManager) {
    return null; // Don't render if not a manager
  }

  const onSubmit = async (data: CsvUploadFormData) => {
    if (!token) {
      setError('Authentication token not available');
      return;
    }

    const file = data.csvFile[0];
    if (!file) {
      setError('Please select a CSV file');
      return;
    }

    setUploading(true);
    setError(null);
    setUploadResult(null);

    try {
      setAuthToken(token);
      const result = await meterReadingsService.uploadCsv(file);
      setUploadResult(result);
      reset();
      onUploadComplete(); // Refresh the meter readings list
    } catch (err: any) {
      setError(err.response?.data?.title || err.message || 'Upload failed');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="card mb-4" data-testid="csv-upload">
      <div className="card-body">
        <h3 className="h5 card-title">Upload Meter Readings (CSV)</h3>
        <form onSubmit={handleSubmit(onSubmit)} className="row g-3 align-items-end">
          <div className="col-md">
            <label htmlFor="csvFile" className="form-label">Select CSV File:</label>
            <input
              type="file"
              id="csvFile"
              accept=".csv"
              {...register('csvFile', { required: 'Please select a CSV file' })}
              className={`form-control ${errors.csvFile ? 'is-invalid' : ''}`}
              data-testid="csv-file-input"
            />
            {errors.csvFile && (
              <div className="invalid-feedback">{errors.csvFile.message}</div>
            )}
          </div>
          <div className="col-auto">
            <button type="submit" disabled={uploading} className="btn btn-primary" data-testid="upload-button">
              {uploading ? 'Uploading...' : 'Upload CSV'}
            </button>
          </div>
        </form>

        {error && (
          <div className="alert alert-danger mt-3" data-testid="error-message">
            <strong>Error:</strong> {error}
          </div>
        )}

        {uploadResult && (
          <div className="mt-3" data-testid="upload-result">
            <div className="alert alert-success">
              <h4 className="h6 mb-2">Upload Complete</h4>
              <p className="mb-1"><strong>Total Processed:</strong> {uploadResult.totalProcessed}</p>
              <p className="mb-1"><strong>Successful:</strong> {uploadResult.successful}</p>
              <p className="mb-0"><strong>Failed:</strong> {uploadResult.failed}</p>
            </div>

            {uploadResult.errors.length > 0 && (
              <div className="mt-2">
                <h5 className="h6">Errors:</h5>
                <ul className="list-group">
                  {uploadResult.errors.map((error, index) => (
                    <li key={index} className="list-group-item list-group-item-danger">
                      <strong>Row {error.row}:</strong> {error.error}
                      {error.rawData && (
                        <div className="small text-muted mt-1">Raw data: {error.rawData}</div>
                      )}
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default CsvUpload;
