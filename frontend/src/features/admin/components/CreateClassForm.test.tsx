import { describe, it, expect, vi, beforeEach } from 'vitest';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { CreateClassForm } from './CreateClassForm';
import { classesApi } from '../../admission-classes/api/classes.api';

vi.mock('../../admission-classes/api/classes.api', () => ({
  classesApi: {
    createClass: vi.fn(),
  },
}));

function renderWithClient(ui: React.ReactElement) {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
      mutations: { retry: false },
    },
  });
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

describe('CreateClassForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders form fields and submit button', () => {
    renderWithClient(<CreateClassForm />);

    expect(screen.getByLabelText(/Class & Shift Name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Total Capacity/i)).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: /Create Admission Class/i })
    ).toBeInTheDocument();
  });

  it('validates empty name and empty seats', async () => {
    renderWithClient(<CreateClassForm />);

    const submitBtn = screen.getByRole('button', { name: /Create Admission Class/i });
    fireEvent.click(submitBtn);

    expect(
      await screen.findByText('Class and shift name is required.')
    ).toBeInTheDocument();
    expect(
      await screen.findByText('Total seats is required.')
    ).toBeInTheDocument();
    expect(classesApi.createClass).not.toHaveBeenCalled();
  });

  it('validates zero or negative total seats', async () => {
    renderWithClient(<CreateClassForm />);

    const nameInput = screen.getByLabelText(/Class & Shift Name/i);
    const seatsInput = screen.getByLabelText(/Total Capacity/i);
    const submitBtn = screen.getByRole('button', { name: /Create Admission Class/i });

    fireEvent.change(nameInput, { target: { value: 'Class 7 - Day Shift' } });
    fireEvent.change(seatsInput, { target: { value: '0' } });
    fireEvent.click(submitBtn);

    expect(
      await screen.findByText('Total seats must be a positive integer (at least 1).')
    ).toBeInTheDocument();
    expect(classesApi.createClass).not.toHaveBeenCalled();
  });

  it('submits valid input, displays success banner, and resets fields', async () => {
    vi.mocked(classesApi.createClass).mockResolvedValueOnce({
      id: 'c_abc123',
      name: 'Class 8 - Morning Shift',
      totalSeats: 45,
    });

    renderWithClient(<CreateClassForm />);

    const nameInput = screen.getByLabelText(/Class & Shift Name/i) as HTMLInputElement;
    const seatsInput = screen.getByLabelText(/Total Capacity/i) as HTMLInputElement;
    const submitBtn = screen.getByRole('button', { name: /Create Admission Class/i });

    fireEvent.change(nameInput, { target: { value: 'Class 8 - Morning Shift' } });
    fireEvent.change(seatsInput, { target: { value: '45' } });
    fireEvent.click(submitBtn);

    await waitFor(() => {
      expect(classesApi.createClass).toHaveBeenCalledWith({
        name: 'Class 8 - Morning Shift',
        totalSeats: 45,
      });
    });

    expect(
      await screen.findByText(/Successfully created admission unit "Class 8 - Morning Shift" with 45 seats/i)
    ).toBeInTheDocument();

    expect(nameInput.value).toBe('');
    expect(seatsInput.value).toBe('');
  });

  it('displays server error banner if API returns an error', async () => {
    vi.mocked(classesApi.createClass).mockRejectedValueOnce(
      new Error('Class with this name already exists.')
    );

    renderWithClient(<CreateClassForm />);

    const nameInput = screen.getByLabelText(/Class & Shift Name/i);
    const seatsInput = screen.getByLabelText(/Total Capacity/i);
    const submitBtn = screen.getByRole('button', { name: /Create Admission Class/i });

    fireEvent.change(nameInput, { target: { value: 'Class 9 - Day' } });
    fireEvent.change(seatsInput, { target: { value: '30' } });
    fireEvent.click(submitBtn);

    expect(
      await screen.findByText('Class with this name already exists.')
    ).toBeInTheDocument();
  });
});

