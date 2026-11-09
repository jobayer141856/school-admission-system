import { useStudentId } from '../../identity/hooks/useStudentId';
import { StudentIdentityBar } from '../../identity/components/StudentIdentityBar';
import { ClipboardList } from 'lucide-react';

export function MyApplicationsPage() {
  const { studentId, hasStudentId } = useStudentId();

  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
          My Applications
        </h1>
        <p className="text-sm text-slate-600 mt-1">
          Review your active reservations, confirm simulated admission fee payment, or cancel an application.
        </p>
      </div>

      <StudentIdentityBar />

      {!hasStudentId ? (
        <div className="bg-white border border-slate-200 rounded-xl p-12 text-center text-slate-500">
          <ClipboardList className="w-12 h-12 text-slate-400 mx-auto mb-3" />
          <h3 className="text-base font-bold text-slate-800">Please identify your student</h3>
          <p className="text-xs text-slate-500 mt-1 max-w-md mx-auto">
            Enter your student birth-registration number above to view active applications and hold statuses.
          </p>
        </div>
      ) : (
        <div className="bg-white border border-slate-200 rounded-xl p-8 text-center text-slate-500">
          <ClipboardList className="w-10 h-10 text-academic-600 mx-auto mb-2" />
          <h3 className="text-base font-bold text-slate-800">Student Applications</h3>
          <p className="text-xs text-slate-500 mt-1">
            Loading applications for student: <strong className="font-mono text-slate-900">{studentId}</strong>
          </p>
        </div>
      )}
    </div>
  );
}

