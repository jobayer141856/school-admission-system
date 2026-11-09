import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { StudentIdentityBar } from '../../identity/components/StudentIdentityBar';
import { ClassList } from '../components/ClassList';
import { useStudentId } from '../../identity/hooks/useStudentId';
import { apiClient, ApiError } from '../../../lib/api-client';
import { useQueryClient } from '@tanstack/react-query';
import { CLASSES_QUERY_KEY } from '../hooks/useClasses';
import { AlertCircle, CheckCircle } from 'lucide-react';

export function AdmissionsPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { hasStudentId } = useStudentId();
  const [isApplyingId, setIsApplyingId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const handleApply = async (classId: string) => {
    if (!hasStudentId) {
      setErrorMessage('Please enter your student birth registration number above before applying.');
      return;
    }

    setErrorMessage(null);
    setSuccessMessage(null);
    setIsApplyingId(classId);

    try {
      await apiClient(`classes/${classId}/holds`, {
        method: 'POST',
      });

      // Invalidate classes to update seat count immediately
      await queryClient.invalidateQueries({ queryKey: CLASSES_QUERY_KEY });

      setSuccessMessage('Seat hold reserved successfully! Redirecting to applications...');
      setTimeout(() => {
        navigate('/my-applications');
      }, 1200);
    } catch (err) {
      if (err instanceof ApiError) {
        setErrorMessage(err.message);
      } else {
        setErrorMessage('Failed to reserve seat. Please try again.');
      }
    } finally {
      setIsApplyingId(null);
    }
  };

  return (
    <div>
      {/* Page Title & Intro */}
      <div className="mb-6">
        <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
          School Admission Intake
        </h1>
        <p className="text-sm text-slate-600 mt-1">
          Select an available class-and-shift admission unit. Seats are held temporarily upon application to allow completion of the admission process.
        </p>
      </div>

      {/* Global Alerts */}
      {errorMessage && (
        <div className="mb-6 bg-rose-50 border border-rose-200 rounded-xl p-4 flex items-start gap-3 text-rose-800 text-sm animate-fadeIn" role="alert">
          <AlertCircle className="w-5 h-5 text-rose-600 shrink-0 mt-0.5" />
          <div className="flex-grow">
            <strong className="font-semibold block">Application Notice</strong>
            <span>{errorMessage}</span>
          </div>
          <button
            type="button"
            onClick={() => setErrorMessage(null)}
            className="text-rose-500 hover:text-rose-700 text-xs font-semibold"
          >
            Dismiss
          </button>
        </div>
      )}

      {successMessage && (
        <div className="mb-6 bg-emerald-50 border border-emerald-200 rounded-xl p-4 flex items-center gap-3 text-emerald-800 text-sm animate-fadeIn" role="status">
          <CheckCircle className="w-5 h-5 text-emerald-600 shrink-0" />
          <span>{successMessage}</span>
        </div>
      )}

      {/* Student Identity Input Card */}
      <StudentIdentityBar />

      {/* Class List & Availability Grid */}
      <ClassList
        hasStudentId={hasStudentId}
        onApply={handleApply}
        isApplyingId={isApplyingId}
      />
    </div>
  );
}
