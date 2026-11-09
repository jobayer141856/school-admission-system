import { useState } from 'react';
import { StudentHoldItem } from '../types/hold.types';
import { HoldCountdown } from './HoldCountdown';
import { CheckCircle2, XCircle, AlertCircle, CreditCard, Ban } from 'lucide-react';

interface ApplicationCardProps {
  hold: StudentHoldItem;
  onConfirm: (holdId: string) => Promise<void>;
  onCancel: (holdId: string) => Promise<void>;
}

export function ApplicationCard({ hold, onConfirm, onCancel }: ApplicationCardProps) {
  const [isProcessing, setIsProcessing] = useState(false);
  const [localStatus, setLocalStatus] = useState(hold.status);

  const status = localStatus;
  const isHeld = status === 'HELD';
  const isConfirmed = status === 'CONFIRMED';
  const isExpired = status === 'EXPIRED';
  const isCancelled = status === 'CANCELLED';

  const handleConfirm = async () => {
    setIsProcessing(true);
    try {
      await onConfirm(hold.holdId);
      setLocalStatus('CONFIRMED');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleCancel = async () => {
    if (!window.confirm('Are you sure you want to cancel this application? The seat will be released immediately.')) {
      return;
    }

    setIsProcessing(true);
    try {
      await onCancel(hold.holdId);
      setLocalStatus('CANCELLED');
    } finally {
      setIsProcessing(false);
    }
  };

  const handleCountdownExpire = () => {
    setLocalStatus('EXPIRED');
  };

  return (
    <div className="bg-white rounded-xl border border-slate-200 shadow-sm hover:shadow transition-shadow p-6">
      {/* Top Header: Class Name, ID & Status Badge */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3 pb-4 border-b border-slate-100">
        <div>
          <span className="text-xs font-mono text-slate-400 block">ID: {hold.holdId}</span>
          <h3 className="text-lg font-bold text-slate-900 tracking-tight">{hold.className}</h3>
        </div>

        <div>
          {isHeld && <HoldCountdown expiresAt={hold.expiresAt} onExpire={handleCountdownExpire} />}

          {isConfirmed && (
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
              <CheckCircle2 className="w-3.5 h-3.5" />
              <span>Admission Confirmed</span>
            </span>
          )}

          {isExpired && (
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200">
              <AlertCircle className="w-3.5 h-3.5" />
              <span>Expired</span>
            </span>
          )}

          {isCancelled && (
            <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-slate-100 text-slate-600 border border-slate-200">
              <XCircle className="w-3.5 h-3.5" />
              <span>Cancelled</span>
            </span>
          )}
        </div>
      </div>

      {/* Body Notice according to status */}
      <div className="py-4">
        {isHeld && (
          <p className="text-xs text-slate-600">
            Seat is currently reserved for your student. Complete the simulated admission fee payment before the timer expires to secure admission.
          </p>
        )}

        {isConfirmed && (
          <div className="bg-emerald-50/50 border border-emerald-100 rounded-lg p-3 text-xs text-emerald-800 flex items-start gap-2">
            <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0 mt-0.5" />
            <div>
              <strong className="font-semibold block">Admission Officially Secured</strong>
              <span>Payment simulation succeeded. Your student's admission is confirmed for this class and shift.</span>
            </div>
          </div>
        )}

        {isExpired && (
          <div className="bg-rose-50/50 border border-rose-100 rounded-lg p-3 text-xs text-rose-800 flex items-start gap-2">
            <AlertCircle className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
            <div>
              <strong className="font-semibold block">Reservation Elapsed</strong>
              <span>This seat hold expired and the seat has been returned to the available pool. Please submit a new application if seats remain.</span>
            </div>
          </div>
        )}

        {isCancelled && (
          <p className="text-xs text-slate-500 italic">
            This reservation was cancelled by the parent. The seat has been returned to the available pool.
          </p>
        )}
      </div>

      {/* Action Buttons */}
      {isHeld && (
        <div className="pt-3 border-t border-slate-100 flex flex-col sm:flex-row items-stretch sm:items-center justify-end gap-3">
          <button
            type="button"
            onClick={handleCancel}
            disabled={isProcessing}
            className="px-4 py-2 rounded-lg text-xs font-medium text-slate-700 bg-white border border-slate-200 hover:bg-slate-50 transition-colors focus:ring-2 focus:ring-slate-400 disabled:opacity-50 flex items-center justify-center gap-1.5"
          >
            <Ban className="w-3.5 h-3.5 text-slate-500" />
            <span>Cancel Application</span>
          </button>

          <button
            type="button"
            onClick={handleConfirm}
            disabled={isProcessing}
            className="px-5 py-2.5 rounded-lg text-xs font-semibold text-white bg-emerald-600 hover:bg-emerald-700 active:bg-emerald-800 transition-colors shadow-sm focus:ring-2 focus:ring-emerald-500 focus:ring-offset-2 disabled:opacity-50 flex items-center justify-center gap-2"
          >
            <CreditCard className="w-4 h-4" />
            <span>{isProcessing ? 'Processing payment...' : 'Pay Admission Fee (Simulated)'}</span>
          </button>
        </div>
      )}

      {isExpired && (
        <div className="pt-3 border-t border-slate-100 flex justify-end">
          <button
            type="button"
            disabled
            className="px-4 py-2 rounded-lg text-xs font-semibold text-slate-400 bg-slate-100 cursor-not-allowed"
          >
            Payment Disabled (Expired)
          </button>
        </div>
      )}
    </div>
  );
}
