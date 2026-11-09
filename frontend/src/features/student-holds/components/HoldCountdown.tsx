import { useState, useEffect } from 'react';
import { Clock } from 'lucide-react';

interface HoldCountdownProps {
  expiresAt: string;
  onExpire?: () => void;
}

export function HoldCountdown({ expiresAt, onExpire }: HoldCountdownProps) {
  const calculateSecondsLeft = () => {
    const expiryTime = new Date(expiresAt).getTime();
    const now = Date.now();
    return Math.max(0, Math.floor((expiryTime - now) / 1000));
  };

  const [secondsLeft, setSecondsLeft] = useState<number>(calculateSecondsLeft);

  useEffect(() => {
    setSecondsLeft(calculateSecondsLeft());

    const interval = setInterval(() => {
      const remaining = calculateSecondsLeft();
      setSecondsLeft(remaining);

      if (remaining <= 0) {
        clearInterval(interval);
        onExpire?.();
      }
    }, 1000);

    return () => clearInterval(interval);
  }, [expiresAt, onExpire]);

  if (secondsLeft <= 0) {
    return (
      <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200">
        <Clock className="w-3.5 h-3.5" />
        <span>Reservation Expired</span>
      </span>
    );
  }

  const minutes = Math.floor(secondsLeft / 60);
  const seconds = secondsLeft % 60;
  const formatted = `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;

  const isUrgent = secondsLeft < 120; // less than 2 minutes

  return (
    <div
      className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold border ${
        isUrgent
          ? 'bg-rose-50 text-rose-700 border-rose-200 animate-pulse'
          : 'bg-amber-50 text-amber-800 border-amber-200'
      }`}
      title={`Reservation expires at ${new Date(expiresAt).toLocaleTimeString()}`}
      role="timer"
      aria-live="polite"
    >
      <Clock className="w-3.5 h-3.5" />
      <span>Expires in</span>
      <span className="font-mono font-bold tracking-tight">{formatted}</span>
    </div>
  );
}
