import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useStudentId } from '../../identity/hooks/useStudentId';
import { StudentIdentityBar } from '../../identity/components/StudentIdentityBar';
import { useMyHolds, useConfirmHold, useCancelHold } from '../hooks/useMyHolds';
import { ApplicationCard } from '../components/ApplicationCard';
import { ClipboardList, AlertTriangle, ArrowRight, CheckCircle, RefreshCw } from 'lucide-react';
import { ApiError } from '../../../lib/api-client';

export function MyApplicationsPage() {
  const { studentId, hasStudentId } = useStudentId();
  const { data: holds, isLoading, isError, error, refetch } = useMyHolds();
  const confirmMutation = useConfirmHold();
  const cancelMutation = useCancelHold();

  const [feedback, setFeedback] = useState<{ type: 'success' | 'error'; message: string } | null>(null);

  const handleConfirm = async (holdId: string) => {
    setFeedback(null);
    try {
      await confirmMutation.mutateAsync(holdId);
      setFeedback({
        type: 'success',
        message: 'Admission payment simulated successfully! Your seat is officially confirmed.',
      });
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Failed to confirm admission.';
      setFeedback({ type: 'error', message });
      throw err;
    }
  };

  const handleCancel = async (holdId: string) => {
    setFeedback(null);
    try {
      await cancelMutation.mutateAsync(holdId);
      setFeedback({
        type: 'success',
        message: 'Your application has been cancelled and the seat hold was released.',
      });
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Failed to cancel application.';
      setFeedback({ type: 'error', message });
      throw err;
    }
  };

  return (
    <div>
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
            My Applications
          </h1>
          <p className="text-sm text-slate-600 mt-1">
            Track your reserved seats, confirm simulated admission fee payments, or release holds.
          </p>
        </div>

        {hasStudentId && (
          <button
            type="button"
            onClick={() => refetch()}
            className="self-start sm:self-auto px-3 py-1.5 rounded-lg border border-slate-200 text-xs font-medium text-slate-600 hover:text-slate-900 hover:bg-slate-100 transition-colors inline-flex items-center gap-1.5"
            title="Refresh applications"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Refresh</span>
          </button>
        )}
      </div>

      {/* Global Alerts */}
      {feedback && (
        <div
          className={`mb-6 p-4 rounded-xl border flex items-start gap-3 text-sm animate-fadeIn ${
            feedback.type === 'success'
              ? 'bg-emerald-50 border-emerald-200 text-emerald-800'
              : 'bg-rose-50 border-rose-200 text-rose-800'
          }`}
          role="status"
        >
          {feedback.type === 'success' ? (
            <CheckCircle className="w-5 h-5 text-emerald-600 shrink-0 mt-0.5" />
          ) : (
            <AlertTriangle className="w-5 h-5 text-rose-600 shrink-0 mt-0.5" />
          )}
          <div className="flex-grow">
            <span>{feedback.message}</span>
          </div>
          <button
            type="button"
            onClick={() => setFeedback(null)}
            className="text-xs font-semibold hover:underline"
          >
            Dismiss
          </button>
        </div>
      )}

      {/* Student Identity Input & Persistence Bar */}
      <StudentIdentityBar />

      {/* Content based on state */}
      {!hasStudentId ? (
        <div className="bg-white border border-slate-200 rounded-xl p-12 text-center text-slate-500">
          <ClipboardList className="w-12 h-12 text-slate-400 mx-auto mb-3" />
          <h3 className="text-base font-bold text-slate-800">Student Identity Required</h3>
          <p className="text-xs text-slate-500 mt-1 max-w-md mx-auto">
            Please enter your student's official birth-registration number above to view active applications and seat reservations.
          </p>
        </div>
      ) : isLoading ? (
        <div className="space-y-4">
          {[1, 2].map((n) => (
            <div key={n} className="bg-white rounded-xl border border-slate-200 p-6 animate-pulse">
              <div className="h-5 bg-slate-200 rounded w-1/3 mb-2" />
              <div className="h-4 bg-slate-100 rounded w-1/4 mb-4" />
              <div className="h-10 bg-slate-100 rounded" />
            </div>
          ))}
        </div>
      ) : isError ? (
        <div className="bg-rose-50 border border-rose-200 rounded-xl p-8 text-center text-rose-800">
          <AlertTriangle className="w-8 h-8 text-rose-600 mx-auto mb-2" />
          <h3 className="text-base font-bold text-rose-900">Failed to load applications</h3>
          <p className="text-xs text-rose-700 mt-1 max-w-md mx-auto">
            {error instanceof Error ? error.message : 'Please check your connection and try again.'}
          </p>
          <button
            type="button"
            onClick={() => refetch()}
            className="mt-4 px-4 py-2 bg-rose-600 text-white rounded-lg text-xs font-semibold hover:bg-rose-700 inline-flex items-center gap-1.5 focus:ring-2 focus:ring-rose-500"
          >
            <RefreshCw className="w-3.5 h-3.5" />
            <span>Try Again</span>
          </button>
        </div>
      ) : !holds || holds.length === 0 ? (
        <div className="bg-white border border-slate-200 rounded-xl p-12 text-center text-slate-500">
          <ClipboardList className="w-12 h-12 text-slate-400 mx-auto mb-3" />
          <h3 className="text-base font-bold text-slate-800">No Applications Found</h3>
          <p className="text-xs text-slate-500 mt-1 max-w-md mx-auto mb-6">
            No seat reservations were found for student <strong className="font-mono text-slate-900">{studentId}</strong>.
          </p>
          <Link
            to="/"
            className="inline-flex items-center gap-2 px-4 py-2.5 bg-academic-700 hover:bg-academic-800 text-white rounded-lg text-xs font-semibold shadow-sm transition-colors focus:ring-2 focus:ring-academic-600"
          >
            <span>Browse Available Classes</span>
            <ArrowRight className="w-4 h-4" />
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          <div className="flex items-center justify-between text-xs text-slate-500 pb-1">
            <span>
              Showing {holds.length} {holds.length === 1 ? 'application' : 'applications'}
            </span>
            <span className="text-[11px] text-slate-400">Live countdown & auto-refreshing</span>
          </div>

          {holds.map((hold) => (
            <ApplicationCard
              key={hold.holdId}
              hold={hold}
              onConfirm={handleConfirm}
              onCancel={handleCancel}
            />
          ))}
        </div>
      )}
    </div>
  );
}
