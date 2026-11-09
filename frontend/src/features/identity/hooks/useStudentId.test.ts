import { describe, it, expect, beforeEach } from 'vitest';
import { renderHook, act } from '@testing-library/react';
import { useStudentId } from './useStudentId';

describe('useStudentId', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  it('initializes with empty string when localStorage is empty', () => {
    const { result } = renderHook(() => useStudentId());
    expect(result.current.studentId).toBe('');
    expect(result.current.hasStudentId).toBe(false);
  });

  it('saves and trims studentId to localStorage', () => {
    const { result } = renderHook(() => useStudentId());

    act(() => {
      result.current.saveStudentId('  20152691234567890  ');
    });

    expect(result.current.studentId).toBe('20152691234567890');
    expect(result.current.hasStudentId).toBe(true);
    expect(localStorage.getItem('student_id')).toBe('20152691234567890');
  });

  it('clears studentId from state and localStorage', () => {
    localStorage.setItem('student_id', '20152691234567890');
    const { result } = renderHook(() => useStudentId());

    act(() => {
      result.current.clearStudentId();
    });

    expect(result.current.studentId).toBe('');
    expect(result.current.hasStudentId).toBe(false);
    expect(localStorage.getItem('student_id')).toBeNull();
  });
});

