import { useState, useEffect } from 'react';

const STORAGE_KEY = 'student_id';

export function useStudentId() {
  const [studentId, setStudentIdState] = useState<string>(() => {
    return localStorage.getItem(STORAGE_KEY) || '';
  });

  useEffect(() => {
    const handleStorageChange = () => {
      const current = localStorage.getItem(STORAGE_KEY) || '';
      setStudentIdState(current);
    };

    window.addEventListener('storage', handleStorageChange);
    return () => window.removeEventListener('storage', handleStorageChange);
  }, []);

  const saveStudentId = (id: string) => {
    const trimmed = id.trim();
    if (trimmed) {
      localStorage.setItem(STORAGE_KEY, trimmed);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
    setStudentIdState(trimmed);
  };

  const clearStudentId = () => {
    localStorage.removeItem(STORAGE_KEY);
    setStudentIdState('');
  };

  return {
    studentId,
    hasStudentId: Boolean(studentId),
    saveStudentId,
    clearStudentId,
  };
}

