import { Link } from 'react-router-dom';
import { Home } from 'lucide-react';

export function NotFoundPage() {
  return (
    <div className="bg-white border border-slate-200 rounded-xl p-16 text-center max-w-lg mx-auto my-12">
      <span className="text-4xl font-extrabold text-academic-700 block mb-2 font-mono">404</span>
      <h2 className="text-xl font-bold text-slate-900 mb-2">Page Not Found</h2>
      <p className="text-xs text-slate-500 mb-6">
        The requested page does not exist or has been moved.
      </p>
      <Link
        to="/"
        className="inline-flex items-center gap-2 px-4 py-2 bg-academic-700 text-white rounded-lg text-xs font-semibold hover:bg-academic-800 transition-colors"
      >
        <Home className="w-4 h-4" />
        <span>Return to Admissions</span>
      </Link>
    </div>
  );
}

