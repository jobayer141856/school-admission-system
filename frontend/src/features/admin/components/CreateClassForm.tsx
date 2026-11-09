import { useState, FormEvent } from 'react';
import { PlusCircle, Loader2, CheckCircle2, AlertCircle } from 'lucide-react';
import { useCreateClass } from '../../admission-classes/hooks/useClasses';

interface FormErrors {
  name?: string;
  totalSeats?: string;
}

export function CreateClassForm() {
  const [name, setName] = useState('');
  const [totalSeats, setTotalSeats] = useState('');
  const [errors, setErrors] = useState<FormErrors>({});
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [serverError, setServerError] = useState<string | null>(null);

  const createClassMutation = useCreateClass();

  const validate = (): boolean => {
    const newErrors: FormErrors = {};
    const trimmedName = name.trim();

    if (!trimmedName) {
      newErrors.name = 'Class and shift name is required.';
    } else if (trimmedName.length > 160) {
      newErrors.name = 'Class name must not exceed 160 characters.';
    }

    const seatsNumber = Number(totalSeats);
    if (!totalSeats || totalSeats.trim() === '') {
      newErrors.totalSeats = 'Total seats is required.';
    } else if (!Number.isInteger(seatsNumber) || seatsNumber <= 0) {
      newErrors.totalSeats = 'Total seats must be a positive integer (at least 1).';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setSuccessMessage(null);
    setServerError(null);

    if (!validate()) {
      return;
    }

    try {
      const created = await createClassMutation.mutateAsync({
        name: name.trim(),
        totalSeats: parseInt(totalSeats.trim(), 10),
      });

      setSuccessMessage(
        `Successfully created admission unit "${created.name}" with ${created.totalSeats} seats.`
      );
      setName('');
      setTotalSeats('');
      setErrors({});
    } catch (err: unknown) {
      if (err instanceof Error) {
        setServerError(err.message);
      } else {
        setServerError('Failed to create admission class. Please verify input and try again.');
      }
    }
  };

  return (
    <div className="bg-white border border-slate-200 rounded-xl p-6 shadow-sm">
      <div className="flex items-center gap-2 mb-4 pb-3 border-b border-slate-100">
        <PlusCircle className="w-5 h-5 text-academic-700" />
        <h2 className="text-lg font-bold text-slate-900">Create Admission Class & Shift</h2>
      </div>

      {successMessage && (
        <div
          role="status"
          className="mb-4 bg-emerald-50 border border-emerald-200 text-emerald-900 rounded-lg p-3 text-sm flex items-start gap-2.5"
        >
          <CheckCircle2 className="w-4 h-4 text-emerald-600 shrink-0 mt-0.5" />
          <div className="flex-1">{successMessage}</div>
          <button
            type="button"
            onClick={() => setSuccessMessage(null)}
            className="text-xs text-emerald-700 hover:text-emerald-900 font-semibold ml-2 cursor-pointer"
          >
            Dismiss
          </button>
        </div>
      )}

      {serverError && (
        <div
          role="alert"
          className="mb-4 bg-rose-50 border border-rose-200 text-rose-900 rounded-lg p-3 text-sm flex items-start gap-2.5"
        >
          <AlertCircle className="w-4 h-4 text-rose-600 shrink-0 mt-0.5" />
          <div className="flex-1">{serverError}</div>
          <button
            type="button"
            onClick={() => setServerError(null)}
            className="text-xs text-rose-700 hover:text-rose-900 font-semibold ml-2 cursor-pointer"
          >
            Dismiss
          </button>
        </div>
      )}

      <form onSubmit={handleSubmit} noValidate className="space-y-4">
        <div>
          <label htmlFor="className" className="block text-xs font-semibold text-slate-700 mb-1">
            Class & Shift Name <span className="text-rose-600">*</span>
          </label>
          <input
            id="className"
            type="text"
            value={name}
            onChange={(e) => {
              setName(e.target.value);
              if (errors.name) setErrors((prev) => ({ ...prev, name: undefined }));
            }}
            placeholder="e.g. Class 6 - Morning Shift"
            maxLength={160}
            className={`w-full px-3 py-2 text-sm rounded-lg border bg-slate-50/50 transition-colors focus:bg-white focus:outline-none focus:ring-2 ${
              errors.name
                ? 'border-rose-400 focus:border-rose-500 focus:ring-rose-200'
                : 'border-slate-300 focus:border-academic-600 focus:ring-academic-100'
            }`}
          />
          {errors.name ? (
            <p className="text-xs text-rose-600 mt-1 font-medium">{errors.name}</p>
          ) : (
            <p className="text-xs text-slate-500 mt-1">
              Descriptive designation including standard, grade, and shift (max 160 characters).
            </p>
          )}
        </div>

        <div>
          <label htmlFor="totalSeats" className="block text-xs font-semibold text-slate-700 mb-1">
            Total Capacity (Seats) <span className="text-rose-600">*</span>
          </label>
          <input
            id="totalSeats"
            type="number"
            min="1"
            step="1"
            value={totalSeats}
            onChange={(e) => {
              setTotalSeats(e.target.value);
              if (errors.totalSeats) setErrors((prev) => ({ ...prev, totalSeats: undefined }));
            }}
            placeholder="e.g. 40"
            className={`w-full px-3 py-2 text-sm rounded-lg border bg-slate-50/50 font-mono transition-colors focus:bg-white focus:outline-none focus:ring-2 ${
              errors.totalSeats
                ? 'border-rose-400 focus:border-rose-500 focus:ring-rose-200'
                : 'border-slate-300 focus:border-academic-600 focus:ring-academic-100'
            }`}
          />
          {errors.totalSeats ? (
            <p className="text-xs text-rose-600 mt-1 font-medium">{errors.totalSeats}</p>
          ) : (
            <p className="text-xs text-slate-500 mt-1">
              Maximum enrollment capacity. Must be an integer greater than 0.
            </p>
          )}
        </div>

        <div className="pt-2">
          <button
            type="submit"
            disabled={createClassMutation.isPending}
            className="inline-flex items-center justify-center gap-2 px-4 py-2.5 rounded-lg text-sm font-semibold text-white bg-academic-700 hover:bg-academic-800 active:bg-academic-900 disabled:opacity-50 disabled:cursor-not-allowed shadow-sm transition-colors cursor-pointer"
          >
            {createClassMutation.isPending ? (
              <>
                <Loader2 className="w-4 h-4 animate-spin" />
                <span>Creating Unit...</span>
              </>
            ) : (
              <>
                <PlusCircle className="w-4 h-4" />
                <span>Create Admission Class</span>
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
}

