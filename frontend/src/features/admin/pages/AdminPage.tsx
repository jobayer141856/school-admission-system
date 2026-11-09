import { ShieldAlert, PlusCircle } from 'lucide-react';

export function AdminPage() {
  return (
    <div>
      <div className="mb-6">
        <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
          School Administration
        </h1>
        <p className="text-sm text-slate-600 mt-1">
          Configure admission class-and-shift units and inspect live capacity.
        </p>
      </div>

      {/* Demo Warning Banner per docs/02-assumptions.md */}
      <div className="mb-8 bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-start gap-3 text-amber-900 text-sm">
        <ShieldAlert className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
        <div>
          <strong className="font-semibold block">Demo Admin Screen</strong>
          <span className="text-xs text-amber-800">
            This administration interface is unauthenticated per project specification assumptions. It is intended for take-home evaluation and demonstration purposes.
          </span>
        </div>
      </div>

      <div className="bg-white border border-slate-200 rounded-xl p-8 text-center text-slate-500">
        <PlusCircle className="w-10 h-10 text-academic-600 mx-auto mb-2" />
        <h3 className="text-base font-bold text-slate-800">Admin Workflow</h3>
        <p className="text-xs text-slate-500 mt-1">
          Admission class creation and live management controls will be connected in Phase 7.
        </p>
      </div>
    </div>
  );
}

