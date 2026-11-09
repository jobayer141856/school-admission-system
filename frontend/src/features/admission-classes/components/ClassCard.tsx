import { AdmissionClassItem } from '../types/class.types';
import { AlertCircle, ArrowRight } from 'lucide-react';

interface ClassCardProps {
  admissionClass: AdmissionClassItem;
  hasStudentId: boolean;
  onApply: (classId: string) => void;
  isApplying?: boolean;
}

export function ClassCard({
  admissionClass,
  hasStudentId,
  onApply,
  isApplying = false,
}: ClassCardProps) {
  const { id, name, totalSeats, heldSeats, confirmedSeats, availableSeats } = admissionClass;

  const isFull = availableSeats <= 0;
  const heldPct = Math.min(100, Math.round((heldSeats / totalSeats) * 100));
  const confirmedPct = Math.min(100, Math.round((confirmedSeats / totalSeats) * 100));

  return (
    <div className="bg-white rounded-xl border border-slate-200 shadow-sm hover:shadow-md transition-shadow p-6 flex flex-col justify-between">
      <div>
        {/* Title & Total Header */}
        <div className="flex items-start justify-between gap-2 mb-3">
          <h3 className="text-base font-bold text-slate-900 tracking-tight leading-snug">
            {name}
          </h3>
          <span className="text-xs font-semibold px-2 py-0.5 rounded-full bg-slate-100 text-slate-700 whitespace-nowrap">
            {totalSeats} Total Seats
          </span>
        </div>

        {/* Capacity Breakdown Pills */}
        <div className="grid grid-cols-3 gap-2 text-center my-4">
          <div className="bg-emerald-50 border border-emerald-100 rounded-lg p-2.5">
            <span className="text-xs font-medium text-emerald-800 block">Available</span>
            <span className="text-xl font-bold text-emerald-700 tracking-tight block">
              {availableSeats}
            </span>
          </div>
          <div className="bg-amber-50 border border-amber-100 rounded-lg p-2.5">
            <span className="text-xs font-medium text-amber-800 block">Held</span>
            <span className="text-xl font-bold text-amber-700 tracking-tight block">
              {heldSeats}
            </span>
          </div>
          <div className="bg-blue-50 border border-blue-100 rounded-lg p-2.5">
            <span className="text-xs font-medium text-blue-800 block">Confirmed</span>
            <span className="text-xl font-bold text-blue-700 tracking-tight block">
              {confirmedSeats}
            </span>
          </div>
        </div>

        {/* Visual Progress Bar */}
        <div className="w-full bg-slate-100 rounded-full h-2 overflow-hidden flex my-3" title={`Confirmed: ${confirmedSeats}, Held: ${heldSeats}, Available: ${availableSeats}`}>
          <div
            className="bg-blue-600 h-full transition-all duration-300"
            style={{ width: `${confirmedPct}%` }}
          />
          <div
            className="bg-amber-500 h-full transition-all duration-300"
            style={{ width: `${heldPct}%` }}
          />
        </div>
      </div>

      {/* Action Footer */}
      <div className="mt-4 pt-4 border-t border-slate-100">
        {isFull ? (
          <div className="w-full py-2.5 px-3 rounded-lg bg-slate-100 text-slate-400 text-xs font-semibold text-center flex items-center justify-center gap-1.5 cursor-not-allowed">
            <AlertCircle className="w-4 h-4" />
            <span>No Seats Available</span>
          </div>
        ) : !hasStudentId ? (
          <button
            type="button"
            disabled
            className="w-full py-2.5 px-3 rounded-lg bg-slate-100 text-slate-500 text-xs font-medium text-center cursor-not-allowed"
            title="Please enter your student birth registration number above first."
          >
            Enter Student ID to Apply
          </button>
        ) : (
          <button
            type="button"
            onClick={() => onApply(id)}
            disabled={isApplying}
            className="w-full py-2.5 px-4 rounded-lg bg-academic-700 hover:bg-academic-800 active:bg-academic-900 text-white text-sm font-medium transition-colors shadow-sm flex items-center justify-center gap-2 focus:outline-none focus:ring-2 focus:ring-academic-600 focus:ring-offset-2 disabled:opacity-50"
          >
            {isApplying ? (
              <span>Reserving seat...</span>
            ) : (
              <>
                <span>Apply for Admission</span>
                <ArrowRight className="w-4 h-4" />
              </>
            )}
          </button>
        )}
      </div>
    </div>
  );
}
