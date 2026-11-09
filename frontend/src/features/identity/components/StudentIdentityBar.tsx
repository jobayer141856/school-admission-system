import { useState, FormEvent } from 'react';
import { useStudentId } from '../hooks/useStudentId';
import { IdCard, CheckCircle2, AlertCircle } from 'lucide-react';

export function StudentIdentityBar() {
  const { studentId, hasStudentId, saveStudentId, clearStudentId } = useStudentId();
  const [inputValue, setInputValue] = useState(studentId);
  const [error, setError] = useState<string | null>(null);
  const [isEditing, setIsEditing] = useState(!hasStudentId);

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    const trimmed = inputValue.trim();

    if (!trimmed) {
      setError('Please enter a valid student birth-registration number.');
      return;
    }

    if (trimmed.length > 64) {
      setError('Student ID cannot exceed 64 characters.');
      return;
    }

    setError(null);
    saveStudentId(trimmed);
    setIsEditing(false);
  };

  const handleEdit = () => {
    setInputValue(studentId);
    setIsEditing(true);
  };

  const handleClear = () => {
    clearStudentId();
    setInputValue('');
    setIsEditing(true);
  };

  return (
    <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 mb-8">
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <div className="flex items-center gap-2 text-academic-700 font-semibold text-sm">
            <IdCard className="w-5 h-5 text-academic-600" />
            <span>Student Identification</span>
          </div>
          <h2 className="text-lg font-bold text-slate-900 mt-1">
            Student Birth-Registration Number
          </h2>
          <p className="text-xs text-slate-500 mt-0.5 max-w-xl">
            This official number identifies your student. It will be sent via the{' '}
            <code className="bg-slate-100 text-slate-800 px-1 py-0.5 rounded font-mono text-xs">X-Student-Id</code>{' '}
            header for seat reservation and admission confirmation.
          </p>
        </div>

        {!isEditing && hasStudentId ? (
          <div className="flex items-center gap-3 bg-slate-50 border border-slate-200 px-4 py-2.5 rounded-lg">
            <div className="flex items-center gap-2">
              <CheckCircle2 className="w-5 h-5 text-emerald-600" />
              <div>
                <span className="text-xs text-slate-500 block">Active Student ID</span>
                <span className="font-mono text-sm font-bold text-slate-900">{studentId}</span>
              </div>
            </div>
            <div className="flex gap-1.5 ml-2">
              <button
                type="button"
                onClick={handleEdit}
                className="px-2.5 py-1 text-xs font-medium text-slate-700 bg-white border border-slate-200 rounded hover:bg-slate-50 transition-colors focus:ring-2 focus:ring-academic-600"
              >
                Change
              </button>
              <button
                type="button"
                onClick={handleClear}
                className="px-2.5 py-1 text-xs font-medium text-rose-600 bg-white border border-rose-200 rounded hover:bg-rose-50 transition-colors focus:ring-2 focus:ring-rose-500"
              >
                Clear
              </button>
            </div>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="flex flex-col sm:flex-row items-stretch sm:items-center gap-2 w-full sm:w-auto">
            <div className="relative">
              <input
                type="text"
                value={inputValue}
                onChange={(e) => {
                  setInputValue(e.target.value);
                  if (error) setError(null);
                }}
                placeholder="e.g. 20152691234567890"
                className={`w-full sm:w-72 px-3.5 py-2 text-sm font-mono border rounded-lg focus:outline-none focus:ring-2 ${
                  error
                    ? 'border-rose-400 focus:ring-rose-500 text-rose-900'
                    : 'border-slate-300 focus:ring-academic-600 focus:border-academic-600'
                }`}
                aria-label="Student birth-registration number"
              />
            </div>
            <button
              type="submit"
              className="px-4 py-2 text-sm font-medium text-white bg-academic-700 hover:bg-academic-800 rounded-lg shadow-sm transition-colors focus:outline-none focus:ring-2 focus:ring-academic-600 focus:ring-offset-2 whitespace-nowrap"
            >
              Set Student ID
            </button>
          </form>
        )}
      </div>

      {error && (
        <div className="mt-3 flex items-center gap-1.5 text-xs text-rose-600" role="alert">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}
    </div>
  );
}

