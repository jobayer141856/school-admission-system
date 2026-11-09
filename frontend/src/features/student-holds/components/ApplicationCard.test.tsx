import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { ApplicationCard } from './ApplicationCard';
import { StudentHoldItem } from '../types/hold.types';

describe('ApplicationCard', () => {
  const futureExpiry = new Date(Date.now() + 600 * 1000).toISOString();

  const mockHeld: StudentHoldItem = {
    holdId: 'h_12345',
    classId: 'c_98765',
    className: 'Class 6 - Morning Shift',
    status: 'HELD',
    expiresAt: futureExpiry,
  };

  beforeEach(() => {
    vi.restoreAllMocks();
    vi.spyOn(window, 'confirm').mockImplementation(() => true);
  });

  it('renders HELD card with active pay and cancel buttons', () => {
    render(<ApplicationCard hold={mockHeld} onConfirm={vi.fn()} onCancel={vi.fn()} />);

    expect(screen.getByText('Class 6 - Morning Shift')).toBeInTheDocument();
    expect(screen.getByText(/ID: h_12345/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Pay Admission Fee/i })).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Cancel Application/i })).toBeInTheDocument();
  });

  it('calls onConfirm when pay button is clicked', async () => {
    const onConfirm = vi.fn().mockResolvedValue(undefined);
    render(<ApplicationCard hold={mockHeld} onConfirm={onConfirm} onCancel={vi.fn()} />);

    const payButton = screen.getByRole('button', { name: /Pay Admission Fee/i });
    fireEvent.click(payButton);

    await waitFor(() => {
      expect(onConfirm).toHaveBeenCalledTimes(1);
      expect(onConfirm).toHaveBeenCalledWith('h_12345');
    });
  });

  it('calls onCancel when cancel button is clicked and confirmed', async () => {
    const onCancel = vi.fn().mockResolvedValue(undefined);
    render(<ApplicationCard hold={mockHeld} onConfirm={vi.fn()} onCancel={onCancel} />);

    const cancelButton = screen.getByRole('button', { name: /Cancel Application/i });
    fireEvent.click(cancelButton);

    await waitFor(() => {
      expect(onCancel).toHaveBeenCalledTimes(1);
      expect(onCancel).toHaveBeenCalledWith('h_12345');
    });
  });

  it('renders CONFIRMED badge and no action buttons for confirmed hold', () => {
    const confirmedHold: StudentHoldItem = {
      ...mockHeld,
      status: 'CONFIRMED',
    };

    render(<ApplicationCard hold={confirmedHold} onConfirm={vi.fn()} onCancel={vi.fn()} />);

    expect(screen.getByText(/Admission Confirmed/i)).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Pay Admission Fee/i })).not.toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /Cancel Application/i })).not.toBeInTheDocument();
  });

  it('disables payment and displays Expired badge for expired hold', () => {
    const expiredHold: StudentHoldItem = {
      ...mockHeld,
      status: 'EXPIRED',
    };

    render(<ApplicationCard hold={expiredHold} onConfirm={vi.fn()} onCancel={vi.fn()} />);

    expect(screen.getByText('Expired')).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Payment Disabled \(Expired\)/i })).toBeDisabled();
  });
});

