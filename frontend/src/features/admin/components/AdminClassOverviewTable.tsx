import { useMemo } from 'react';
import { Layers, Users, Clock, CheckCircle2, AlertCircle, RefreshCw } from 'lucide-react';
import { AdmissionClassItem } from '../../admission-classes/types/class.types';

interface AdminClassOverviewTableProps {
  classes: AdmissionClassItem[];
  isLoading: boolean;
  onRefresh?: () => void;
}

export function AdminClassOverviewTable({
  classes,
  isLoading,
  onRefresh,
}: AdminClassOverviewTableProps) {
  const stats = useMemo(() => {
    const totalUnits = classes.length;
    const totalSeats = classes.reduce((sum, c) => sum + c.totalSeats, 0);
    const totalHeld = classes.reduce((sum, c) => sum + c.heldSeats, 0);
    const totalConfirmed = classes.reduce((sum, c) => sum + c.confirmedSeats, 0);
    const totalAvailable = classes.reduce((sum, c) => sum + c.availableSeats, 0);
    const occupancyRate = totalSeats > 0 ? Math.round(((totalConfirmed + totalHeld) / totalSeats) * 100) : 0;

    return {
      totalUnits,
      totalSeats,
      totalHeld,
      totalConfirmed,
      totalAvailable,
      occupancyRate,
    };
  }, [classes]);

  return (
    <div className="space-y-6">
      {/* Metrics Row */}
      <div className="grid grid-cols-2 lg:grid-cols-5 gap-3">
        <div className="bg-white border border-slate-200 rounded-xl p-3.5 shadow-sm">
          <div className="flex items-center justify-between text-slate-500 mb-1">
            <span className="text-xs font-semibold uppercase tracking-wider">Admission Units</span>
            <Layers className="w-4 h-4 text-slate-400" />
          </div>
          <div className="text-2xl font-black text-slate-900 font-mono tabular-nums">
            {isLoading ? '—' : stats.totalUnits}
          </div>
        </div>

        <div className="bg-white border border-slate-200 rounded-xl p-3.5 shadow-sm">
          <div className="flex items-center justify-between text-slate-500 mb-1">
            <span className="text-xs font-semibold uppercase tracking-wider">Total Seats</span>
            <Users className="w-4 h-4 text-academic-600" />
          </div>
          <div className="text-2xl font-black text-slate-900 font-mono tabular-nums">
            {isLoading ? '—' : stats.totalSeats}
          </div>
        </div>

        <div className="bg-white border border-amber-200/80 rounded-xl p-3.5 shadow-sm bg-gradient-to-br from-white to-amber-50/30">
          <div className="flex items-center justify-between text-amber-700 mb-1">
            <span className="text-xs font-semibold uppercase tracking-wider">Active Holds</span>
            <Clock className="w-4 h-4 text-amber-500" />
          </div>
          <div className="text-2xl font-black text-amber-900 font-mono tabular-nums">
            {isLoading ? '—' : stats.totalHeld}
          </div>
        </div>

        <div className="bg-white border border-emerald-200/80 rounded-xl p-3.5 shadow-sm bg-gradient-to-br from-white to-emerald-50/30">
          <div className="flex items-center justify-between text-emerald-700 mb-1">
            <span className="text-xs font-semibold uppercase tracking-wider">Confirmed</span>
            <CheckCircle2 className="w-4 h-4 text-emerald-500" />
          </div>
          <div className="text-2xl font-black text-emerald-900 font-mono tabular-nums">
            {isLoading ? '—' : stats.totalConfirmed}
          </div>
        </div>

        <div className="col-span-2 lg:col-span-1 bg-white border border-indigo-200/80 rounded-xl p-3.5 shadow-sm bg-gradient-to-br from-white to-indigo-50/30">
          <div className="flex items-center justify-between text-indigo-700 mb-1">
            <span className="text-xs font-semibold uppercase tracking-wider">Available</span>
            <span className="text-xs font-bold text-indigo-600 font-mono">{stats.occupancyRate}% full</span>
          </div>
          <div className="text-2xl font-black text-indigo-950 font-mono tabular-nums">
            {isLoading ? '—' : stats.totalAvailable}
          </div>
        </div>
      </div>

      {/* Overview Table */}
      <div className="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h2 className="text-base font-bold text-slate-900">Admission Units & Capacity Status</h2>
            <p className="text-xs text-slate-500 mt-0.5">
              Live breakdown of seats, active countdown holds, and finalized enrollments.
            </p>
          </div>
          {onRefresh && (
            <button
              type="button"
              onClick={onRefresh}
              className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-slate-600 hover:text-slate-900 hover:bg-slate-100 rounded-lg transition-colors cursor-pointer"
            >
              <RefreshCw className={`w-3.5 h-3.5 ${isLoading ? 'animate-spin' : ''}`} />
              <span>Refresh</span>
            </button>
          )}
        </div>

        {isLoading ? (
          <div className="p-8 text-center text-slate-500">
            <RefreshCw className="w-6 h-6 animate-spin text-academic-600 mx-auto mb-2" />
            <p className="text-sm font-medium">Loading admission classes...</p>
          </div>
        ) : classes.length === 0 ? (
          <div className="p-10 text-center">
            <AlertCircle className="w-8 h-8 text-slate-400 mx-auto mb-2" />
            <h3 className="text-sm font-bold text-slate-800">No Admission Classes Configured</h3>
            <p className="text-xs text-slate-500 mt-1 max-w-sm mx-auto">
              Use the creation form above to configure your first admission class and shift.
            </p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-left text-sm border-collapse">
              <thead>
                <tr className="bg-slate-50/75 border-b border-slate-200 text-slate-600 text-xs font-semibold uppercase tracking-wider">
                  <th scope="col" className="px-6 py-3">Class & Shift</th>
                  <th scope="col" className="px-4 py-3 text-right">Total</th>
                  <th scope="col" className="px-4 py-3 text-right">Held</th>
                  <th scope="col" className="px-4 py-3 text-right">Confirmed</th>
                  <th scope="col" className="px-4 py-3 text-right">Available</th>
                  <th scope="col" className="px-6 py-3 min-w-[180px]">Occupancy</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {classes.map((cls) => {
                  const confirmedPct = (cls.confirmedSeats / cls.totalSeats) * 100;
                  const heldPct = (cls.heldSeats / cls.totalSeats) * 100;
                  const isFull = cls.availableSeats <= 0;

                  return (
                    <tr key={cls.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-6 py-4">
                        <div className="font-bold text-slate-900">{cls.name}</div>
                        <div className="text-[11px] font-mono text-slate-400 mt-0.5">{cls.id}</div>
                      </td>
                      <td className="px-4 py-4 text-right font-mono font-bold text-slate-800 tabular-nums">
                        {cls.totalSeats}
                      </td>
                      <td className="px-4 py-4 text-right">
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold font-mono bg-amber-50 text-amber-700 border border-amber-200">
                          {cls.heldSeats}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-right">
                        <span className="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold font-mono bg-emerald-50 text-emerald-700 border border-emerald-200">
                          {cls.confirmedSeats}
                        </span>
                      </td>
                      <td className="px-4 py-4 text-right">
                        <span
                          className={`inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold font-mono border ${
                            isFull
                              ? 'bg-slate-100 text-slate-600 border-slate-300'
                              : 'bg-indigo-50 text-indigo-700 border-indigo-200'
                          }`}
                        >
                          {cls.availableSeats}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        <div className="space-y-1.5">
                          <div className="h-2 w-full bg-slate-100 rounded-full overflow-hidden flex">
                            <div
                              style={{ width: `${confirmedPct}%` }}
                              className="bg-emerald-500 h-full transition-all"
                              title={`Confirmed: ${cls.confirmedSeats}`}
                            />
                            <div
                              style={{ width: `${heldPct}%` }}
                              className="bg-amber-400 h-full transition-all"
                              title={`Held: ${cls.heldSeats}`}
                            />
                          </div>
                          <div className="flex items-center justify-between text-[11px] text-slate-500">
                            <span>
                              {Math.round(confirmedPct + heldPct)}% allocated
                            </span>
                            {isFull && (
                              <span className="font-bold text-rose-600">Full</span>
                            )}
                          </div>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}

