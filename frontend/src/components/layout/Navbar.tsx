import { Link, useLocation } from 'react-router-dom';
import { GraduationCap, User, ShieldCheck } from 'lucide-react';
import { useStudentId } from '../../features/identity/hooks/useStudentId';

export function Navbar() {
  const location = useLocation();
  const { studentId } = useStudentId();

  const navLinks = [
    { href: '/', label: 'Available Classes' },
    { href: '/my-applications', label: 'My Applications' },
    { href: '/admin', label: 'Admin (Demo)' },
  ];

  return (
    <header className="bg-white border-b border-slate-200 sticky top-0 z-30">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16">
          {/* Logo & Brand */}
          <Link to="/" className="flex items-center gap-3 group focus:outline-none focus:ring-2 focus:ring-academic-600 rounded-md p-1">
            <div className="w-10 h-10 rounded-lg bg-academic-700 text-white flex items-center justify-center shadow-sm group-hover:bg-academic-800 transition-colors">
              <GraduationCap className="w-6 h-6" />
            </div>
            <div>
              <span className="text-base font-bold text-slate-900 tracking-tight block">School Admission</span>
              <span className="text-xs text-slate-500 font-medium block">Academic Year 2026-2027</span>
            </div>
          </Link>

          {/* Navigation Links */}
          <nav className="flex items-center gap-1 sm:gap-2">
            {navLinks.map((link) => {
              const isActive = location.pathname === link.href;
              return (
                <Link
                  key={link.href}
                  to={link.href}
                  className={`px-3 py-2 rounded-lg text-sm font-medium transition-colors focus:outline-none focus:ring-2 focus:ring-academic-600 ${
                    isActive
                      ? 'bg-academic-50 text-academic-700 font-semibold'
                      : 'text-slate-600 hover:text-slate-900 hover:bg-slate-100'
                  }`}
                >
                  {link.label}
                </Link>
              );
            })}
          </nav>

          {/* Student ID Indicator */}
          <div className="hidden md:flex items-center gap-2 pl-4 border-l border-slate-200">
            {studentId ? (
              <div className="flex items-center gap-2 px-3 py-1.5 rounded-full bg-slate-100 border border-slate-200 text-xs font-medium text-slate-700">
                <ShieldCheck className="w-4 h-4 text-emerald-600" />
                <span>Student: <strong className="font-mono text-slate-900">{studentId}</strong></span>
              </div>
            ) : (
              <div className="flex items-center gap-1.5 text-xs text-amber-700 bg-amber-50 px-2.5 py-1 rounded-full border border-amber-200">
                <User className="w-3.5 h-3.5" />
                <span>No Student ID</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}

