import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, act } from '@testing-library/react';
import { HoldCountdown } from './HoldCountdown';

describe('HoldCountdown', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('renders minutes and seconds remaining', () => {
    const now = Date.now();
    vi.setSystemTime(now);

    const expiresAt = new Date(now + 125 * 1000).toISOString(); // 2m 5s

    render(<HoldCountdown expiresAt={expiresAt} />);

    expect(screen.getByText('02:05')).toBeInTheDocument();
    expect(screen.getByText(/Expires in/i)).toBeInTheDocument();
  });

  it('counts down every second and calls onExpire when reaching zero', () => {
    const now = Date.now();
    vi.setSystemTime(now);

    const onExpire = vi.fn();
    const expiresAt = new Date(now + 2 * 1000).toISOString(); // 2 seconds

    render(<HoldCountdown expiresAt={expiresAt} onExpire={onExpire} />);

    expect(screen.getByText('00:02')).toBeInTheDocument();

    act(() => {
      vi.advanceTimersByTime(1000);
    });

    expect(screen.getByText('00:01')).toBeInTheDocument();
    expect(onExpire).not.toHaveBeenCalled();

    act(() => {
      vi.advanceTimersByTime(1000);
    });

    expect(screen.getByText(/Reservation Expired/i)).toBeInTheDocument();
    expect(onExpire).toHaveBeenCalledTimes(1);
  });

  it('renders Reservation Expired immediately when past expiry', () => {
    const now = Date.now();
    vi.setSystemTime(now);

    const pastExpiry = new Date(now - 1000).toISOString();
    render(<HoldCountdown expiresAt={pastExpiry} />);

    expect(screen.getByText(/Reservation Expired/i)).toBeInTheDocument();
  });
});

