import { useClasses } from '../hooks/useClasses';
import { ClassCard } from './ClassCard';
import { School, RefreshCw, AlertTriangle } from 'lucide-react';

interface ClassListProps {
  hasStudentId: boolean;
  onApply: (classId: string) => void;
  isApplyingId?: string | null;
}

export function ClassList({ hasStudentId, onApply, isApplyingId }: ClassListProps) {
  const { data: classes, isLoading, isError, error, refetch } = useClasses();

  if (isLoading) {
    return (
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {[1, 2, 3].map((n) => (
          <div key={n} className="bg-white rounded-xl border border-slate-200 p-6 animate-pulse">
            <div className="h-5 bg-slate-200 rounded w-2/3 mb-4" />
            <div className="grid grid-cols-3 gap-2 my-4">
              <div className="h-16 bg-slate-100 rounded-lg" />
              <div className="h-16 bg-slate-100 rounded-lg" />
              <div className="h-16 bg-slate-100 rounded-lg" />
            </div>
            <div className="h-2 bg-slate-100 rounded-full my-3" />
            <div className="h-9 bg-slate-200 rounded-lg mt-4" />
          </div>
        ))}
      </div>
    );
  }

  if (isError) {
    return (
      <div className="bg-rose-50 border border-rose-200 rounded-xl p-8 text-center text-rose-800">
        <AlertTriangle className="w-8 h-8 text-rose-600 mx-auto mb-2" />
        <h3 className="text-base font-bold text-rose-900">Failed to load admission classes</h3>
        <p className="text-xs text-rose-700 mt-1 max-w-md mx-auto">
          {error instanceof Error ? error.message : 'Please check that the backend API is running.'}
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
    );
  }

  if (!classes || classes.length === 0) {
    return (
      <div className="bg-white border border-slate-200 rounded-xl p-12 text-center text-slate-500">
        <School className="w-12 h-12 text-slate-400 mx-auto mb-3" />
        <h3 className="text-base font-bold text-slate-800">No Admission Classes Found</h3>
        <p className="text-xs text-slate-500 mt-1 max-w-md mx-auto">
          No classes have been configured by administrators yet. Please check back later or use the Admin panel to create units.
        </p>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-xl font-bold text-slate-900 tracking-tight">Available Admission Classes</h2>
          <p className="text-xs text-slate-500">First-come-first-served capacity allocation with live countdown seats</p>
        </div>
        <button
          type="button"
          onClick={() => refetch()}
          className="p-2 text-slate-500 hover:text-slate-700 hover:bg-slate-100 rounded-lg text-xs font-medium inline-flex items-center gap-1 transition-colors"
          title="Refresh class capacities"
        >
          <RefreshCw className="w-4 h-4" />
          <span className="hidden sm:inline">Refresh</span>
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        {classes.map((c) => (
          <ClassCard
            key={c.id}
            admissionClass={c}
            hasStudentId={hasStudentId}
            onApply={onApply}
            isApplying={isApplyingId === c.id}
          />
        ))}
      </div>
    </div>
  );
}

