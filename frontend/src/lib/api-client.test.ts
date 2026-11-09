import { describe, it, expect, beforeEach, vi } from 'vitest';
import { apiClient, ApiError } from './api-client';

describe('apiClient', () => {
  beforeEach(() => {
    localStorage.clear();
    vi.restoreAllMocks();
  });

  it('attaches X-Student-Id header when stored in localStorage', async () => {
    localStorage.setItem('student_id', '20152691234567890');

    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: async () => ({ id: 'c_1', name: 'Class 6' }),
    } as Response);

    await apiClient('classes');

    expect(fetchSpy).toHaveBeenCalledTimes(1);
    const [, options] = fetchSpy.mock.calls[0];
    const headers = options?.headers as Headers;
    expect(headers.get('X-Student-Id')).toBe('20152691234567890');
  });

  it('throws ApiError with problem details on error response', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: false,
      status: 409,
      headers: new Headers({ 'content-type': 'application/problem+json' }),
      json: async () => ({
        status: 409,
        title: 'Seat is no longer available',
        detail: 'No available seats remain for this admission class.',
      }),
    } as Response);

    await expect(apiClient('classes/c_1/holds', { method: 'POST' })).rejects.toThrow(ApiError);
  });

  it('handles 204 No Content cleanly', async () => {
    vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      status: 204,
      headers: new Headers(),
    } as Response);

    const result = await apiClient('holds/h_1', { method: 'DELETE' });
    expect(result).toBeUndefined();
  });
});
