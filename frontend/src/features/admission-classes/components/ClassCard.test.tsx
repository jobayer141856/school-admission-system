import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { ClassCard } from './ClassCard';
import { AdmissionClassItem } from '../types/class.types';

describe('ClassCard', () => {
  const mockClass: AdmissionClassItem = {
    id: 'c_123',
    name: 'Class 6 - Morning Shift',
    totalSeats: 120,
    heldSeats: 15,
    confirmedSeats: 80,
    availableSeats: 25,
  };

  it('renders class name and seat counters accurately', () => {
    render(
      <ClassCard
        admissionClass={mockClass}
        hasStudentId={true}
        onApply={vi.fn()}
      />
    );

    expect(screen.getByText('Class 6 - Morning Shift')).toBeInTheDocument();
    expect(screen.getByText('120 Total Seats')).toBeInTheDocument();
    expect(screen.getByText('25')).toBeInTheDocument(); // available
    expect(screen.getByText('15')).toBeInTheDocument(); // held
    expect(screen.getByText('80')).toBeInTheDocument(); // confirmed
  });

  it('calls onApply with classId when Apply button is clicked', () => {
    const onApply = vi.fn();
    render(
      <ClassCard
        admissionClass={mockClass}
        hasStudentId={true}
        onApply={onApply}
      />
    );

    const applyButton = screen.getByRole('button', { name: /Apply for Admission/i });
    fireEvent.click(applyButton);

    expect(onApply).toHaveBeenCalledTimes(1);
    expect(onApply).toHaveBeenCalledWith('c_123');
  });

  it('disables apply and prompts when no student ID is entered', () => {
    render(
      <ClassCard
        admissionClass={mockClass}
        hasStudentId={false}
        onApply={vi.fn()}
      />
    );

    const button = screen.getByRole('button', { name: /Enter Student ID to Apply/i });
    expect(button).toBeDisabled();
  });

  it('shows No Seats Available when available seats are 0', () => {
    const fullClass: AdmissionClassItem = {
      ...mockClass,
      availableSeats: 0,
      heldSeats: 40,
      confirmedSeats: 80,
    };

    render(
      <ClassCard
        admissionClass={fullClass}
        hasStudentId={true}
        onApply={vi.fn()}
      />
    );

    expect(screen.getByText(/No Seats Available/i)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Apply for Admission/i })).not.toBeInTheDocument();
  });
});

