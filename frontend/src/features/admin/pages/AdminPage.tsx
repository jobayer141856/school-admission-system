import { ShieldAlert } from 'lucide-react';
import { useClasses } from '../../admission-classes/hooks/useClasses';
import { CreateClassForm } from '../components/CreateClassForm';
import { AdminClassOverviewTable } from '../components/AdminClassOverviewTable';

export function AdminPage() {
  const { data: classes, isLoading, refetch } = useClasses();

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-2xl sm:text-3xl font-extrabold text-slate-900 tracking-tight">
          School Administration
        </h1>
        <p className="text-sm text-slate-600 mt-1">
          Configure admission class-and-shift units, set seat capacities, and monitor live allocation metrics.
        </p>
      </div>

      {/* Demo Warning Banner per docs/02-assumptions.md */}
      <div className="bg-amber-50 border border-amber-200 rounded-xl p-4 flex items-start gap-3 text-amber-900 text-sm">
        <ShieldAlert className="w-5 h-5 text-amber-600 shrink-0 mt-0.5" />
        <div>
          <strong className="font-semibold block">Demo Admin Screen</strong>
          <span className="text-xs text-amber-800">
            This administration interface is unauthenticated per project specification assumptions. It is intended for take-home evaluation and demonstration purposes. In production, this view would be guarded by role-based access control (RBAC).
          </span>
        </div>
      </div>

      {/* Main Admin Content Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-start">
        {/* Left Column: Create Form */}
        <div className="lg:col-span-4 lg:sticky lg:top-20">
          <CreateClassForm />
        </div>

        {/* Right Column: Metrics & Overview Table */}
        <div className="lg:col-span-8">
          <AdminClassOverviewTable
            classes={classes ?? []}
            isLoading={isLoading}
            onRefresh={() => refetch()}
          />
        </div>
      </div>
    </div>
  );
}
