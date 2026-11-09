import { describe, it, expect, vi } from 'vitest';
import { render, screen, fireEvent } from '@testing-library/react';
import { AdminClassOverviewTable } from './AdminClassOverviewTable';
import { AdmissionClassItem } from '../../admission-classes/types/class.types';

describe('AdminClassOverviewTable', () => {
  const mockClasses: AdmissionClassItem[] = [
    {
      id: 'c_11111111-1111-1111-1111-111111111111',
      name: 'Class 6 - Morning Shift',
      totalSeats: 30,
      heldSeats: 5,
      confirmedSeats: 10,
      availableSeats: 15,
    },
    {
      id: 'c_22222222-2222-2222-2222-222222222222',
      name: 'Class 7 - Day Shift',
      totalSeats: 20,
      heldSeats: 2,
      confirmedSeats: 18,
      availableSeats: 0,
    },
  ];

  it('renders loading state when isLoading is true', () => {
    render(<AdminClassOverviewTable classes={[]} isLoading={true} />);

    expect(screen.getByText(/Loading admission classes.../i)).toBeInTheDocument();
  });

  it('renders empty state when classes array is empty', () => {
    render(<AdminClassOverviewTable classes={[]} isLoading={false} />);

    expect(
      screen.getByText(/No Admission Classes Configured/i)
    ).toBeInTheDocument();
  });

  it('calculates and renders aggregate metrics and table rows', () => {
    render(<AdminClassOverviewTable classes={mockClasses} isLoading={false} />);

    // Aggregate metrics (2 units total, 50 total seats, 7 held, 28 confirmed, 15 available)
    expect(screen.getAllByText('2').length).toBeGreaterThanOrEqual(1);
    expect(screen.getByText('50')).toBeInTheDocument();
    expect(screen.getByText('7')).toBeInTheDocument();
    expect(screen.getByText('28')).toBeInTheDocument();
    expect(screen.getAllByText('15').length).toBeGreaterThanOrEqual(1);

    // Table rows
    expect(screen.getByText('Class 6 - Morning Shift')).toBeInTheDocument();
    expect(screen.getByText('Class 7 - Day Shift')).toBeInTheDocument();
    expect(screen.getByText('c_11111111-1111-1111-1111-111111111111')).toBeInTheDocument();
    expect(screen.getByText('c_22222222-2222-2222-2222-222222222222')).toBeInTheDocument();

    // Zero available marks Full
    expect(screen.getByText('Full')).toBeInTheDocument();
  });

  it('calls onRefresh when refresh button is clicked', () => {
    const onRefresh = vi.fn();
    render(
      <AdminClassOverviewTable
        classes={mockClasses}
        isLoading={false}
        onRefresh={onRefresh}
      />
    );

    const refreshBtn = screen.getByRole('button', { name: /Refresh/i });
    fireEvent.click(refreshBtn);

    expect(onRefresh).toHaveBeenCalledTimes(1);
  });
});
