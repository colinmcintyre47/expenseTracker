import { useCallback, useEffect, useRef, useState } from 'react';
import toast from 'react-hot-toast';
import { statementsApi } from '../api/statements';
import { UploadedStatement, UploadResult } from '../types';
import LoadingSpinner from '../components/Common/LoadingSpinner';
import { formatDate } from '../utils/formatters';
import './UploadPage.css';

const SUPPORTED_BANKS = ['PNC']; // Matches BankParserFactory in backend

// Generate year options: current year back to 5 years ago
const YEAR_OPTIONS = Array.from({ length: 6 }, (_, i) => new Date().getFullYear() - i);

/**
 * Upload page — lets users import CSV or PDF bank statements.
 * Shows a drag-and-drop area, bank selector, year picker, and past upload history.
 */
const UploadPage: React.FC = () => {
  const [file, setFile] = useState<File | null>(null);
  const [bank, setBank] = useState('PNC');
  const [year, setYear] = useState(new Date().getFullYear() - 1); // default to last year
  const [uploading, setUploading] = useState(false);
  const [result, setResult] = useState<UploadResult | null>(null);
  const [statements, setStatements] = useState<UploadedStatement[]>([]);
  const [dragging, setDragging] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  const loadHistory = () =>
    statementsApi.getAll().then(setStatements).catch(console.error);

  useEffect(() => { loadHistory(); }, []);

  const handleFile = (f: File) => {
    const ext = f.name.split('.').pop()?.toLowerCase();
    if (ext !== 'csv' && ext !== 'pdf') {
      toast.error('Please upload a CSV or PDF file');
      return;
    }
    setFile(f);
    setResult(null);
  };

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault();
    setDragging(false);
    const dropped = e.dataTransfer.files[0];
    if (dropped) handleFile(dropped);
  }, []);

  const handleUpload = async () => {
    if (!file) return;
    setUploading(true);
    try {
      const res = await statementsApi.upload(file, bank, year);
      setResult(res);
      setFile(null);
      loadHistory();
      if (res.importedCount > 0) {
        toast.success(`Imported ${res.importedCount} transactions!`);
      } else {
        toast(`No new transactions found (${res.duplicateCount} duplicates skipped)`, { icon: 'ℹ' });
      }
    } catch {
      toast.error('Upload failed. Please check your file and try again.');
    } finally {
      setUploading(false);
    }
  };

  return (
    <div className="upload-page fade-in">
      <div className="upload-grid">
        {/* Upload form */}
        <div className="card">
          <h3 className="card-title">Import Bank Statement</h3>

          {/* Bank + Year row */}
          <div style={{ display: 'flex', gap: 'var(--space-md)' }}>
            <div className="form-group" style={{ flex: 2 }}>
              <label className="form-label">Bank</label>
              <select className="form-select" value={bank} onChange={e => setBank(e.target.value)}>
                {SUPPORTED_BANKS.map(b => <option key={b} value={b}>{b}</option>)}
              </select>
            </div>
            <div className="form-group" style={{ flex: 1 }}>
              <label className="form-label">Statement Year</label>
              <select className="form-select" value={year} onChange={e => setYear(Number(e.target.value))}>
                {YEAR_OPTIONS.map(y => <option key={y} value={y}>{y}</option>)}
              </select>
            </div>
          </div>

          {/* Drag-and-drop zone */}
          <div
            className={`upload-dropzone ${dragging ? 'dragging' : ''} ${file ? 'has-file' : ''}`}
            onDragOver={e => { e.preventDefault(); setDragging(true); }}
            onDragLeave={() => setDragging(false)}
            onDrop={handleDrop}
            onClick={() => fileRef.current?.click()}
          >
            <input
              ref={fileRef}
              type="file"
              accept=".csv,.pdf"
              style={{ display: 'none' }}
              onChange={e => e.target.files?.[0] && handleFile(e.target.files[0])}
            />
            {file ? (
              <div className="upload-file-selected">
                <div className="upload-file-icon">📄</div>
                <div className="upload-file-name">{file.name}</div>
                <div className="upload-file-size">{(file.size / 1024).toFixed(1)} KB</div>
              </div>
            ) : (
              <div className="upload-placeholder">
                <div className="upload-placeholder-icon">↑</div>
                <div className="upload-placeholder-text">
                  <strong>Click to browse</strong> or drag and drop
                </div>
                <div className="upload-placeholder-hint">CSV or PDF files</div>
              </div>
            )}
          </div>

          <button
            className="btn btn-primary"
            disabled={!file || uploading}
            onClick={handleUpload}
            style={{ marginTop: 'var(--space-md)', width: '100%', justifyContent: 'center' }}
          >
            {uploading ? <><LoadingSpinner size="sm" /> Processing…</> : 'Import Statement'}
          </button>

          {/* Upload result summary */}
          {result && (
            <div className="upload-result">
              <div className="upload-result-row success">
                ✓ {result.importedCount} transactions imported
              </div>
              {result.duplicateCount > 0 && (
                <div className="upload-result-row info">
                  ℹ {result.duplicateCount} duplicates skipped
                </div>
              )}
              {result.errors.length > 0 && (
                <div className="upload-result-row error">
                  ⚠ {result.errors.length} errors
                  <ul className="upload-errors">
                    {result.errors.slice(0, 5).map((e, i) => <li key={i}>{e}</li>)}
                  </ul>
                </div>
              )}
            </div>
          )}
        </div>

        {/* PNC format guide */}
        <div className="card">
          <h3 className="card-title">PNC Import Guide</h3>
          <p style={{ fontSize: '0.875rem', color: 'var(--color-text-secondary)', marginBottom: 'var(--space-md)' }}>
            Download your statement from PNC Online Banking:
          </p>
          <ol className="upload-instructions">
            <li>Log in to PNC Online Banking</li>
            <li>Go to Accounts → Account Activity</li>
            <li>Select your date range</li>
            <li>Click <strong>Download Activity</strong> → choose <strong>PDF</strong> or <strong>CSV</strong></li>
          </ol>

          <div className="upload-sample">
            <div className="upload-sample-title">Supported formats:</div>
            <pre className="upload-sample-code">
{`PDF  — e.g. statement_2025-02.pdf
CSV  — e.g. statement_2025-02.csv`}
            </pre>
          </div>
        </div>
      </div>

      {/* Upload history */}
      {statements.length > 0 && (
        <div className="card">
          <h3 className="card-title">Upload History</h3>
          <table className="upload-history-table">
            <thead>
              <tr>
                <th>Bank</th>
                <th>File</th>
                <th>Uploaded</th>
                <th>Transactions</th>
                <th>Status</th>
              </tr>
            </thead>
            <tbody>
              {statements.map(s => (
                <tr key={s.id}>
                  <td>{s.bankName}</td>
                  <td>{s.fileName}</td>
                  <td>{formatDate(s.uploadedAt)}</td>
                  <td>{s.transactionCount}</td>
                  <td>
                    <span className={`badge ${
                      s.status === 'Completed' ? 'badge-success' :
                      s.status === 'Failed' ? 'badge-error' : 'badge-warning'
                    }`}>
                      {s.status}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default UploadPage;
