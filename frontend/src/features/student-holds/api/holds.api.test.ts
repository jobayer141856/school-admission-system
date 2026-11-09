import { describe, it, expect, vi, beforeEach } from 'vitest';
import { holdsApi } from './holds.api';

describe('holdsApi', () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it('getMyHolds calls me/holds endpoint', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: async () => [{ holdId: 'h_1', className: 'Class 6', status: 'HELD' }],
    } as Response);

    const holds = await holdsApi.getMyHolds();

    expect(fetchSpy).toHaveBeenCalledTimes(1);
    const [url] = fetchSpy.mock.calls[0];
    expect(url).toContain('me/holds');
    expect(holds).toHaveLength(1);
  });

  it('confirmHold sends POST to holds/:holdId/confirm', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      status: 200,
      headers: new Headers({ 'content-type': 'application/json' }),
      json: async () => ({ holdId: 'h_1', status: 'CONFIRMED' }),
    } as Response);

    const result = await holdsApi.confirmHold('h_1');

    expect(fetchSpy).toHaveBeenCalledTimes(1);
    const [url, options] = fetchSpy.mock.calls[0];
    expect(url).toContain('holds/h_1/confirm');
    expect(options?.method).toBe('POST');
    expect(result.status).toBe('CONFIRMED');
  });

  it('cancelHold sends DELETE to holds/:holdId', async () => {
    const fetchSpy = vi.spyOn(globalThis, 'fetch').mockResolvedValueOnce({
      ok: true,
      status: 204,
      headers: new Headers(),
    } as Response);

    await holdsApi.cancelHold('h_1');

    expect(fetchSpy).toHaveBeenCalledTimes(1);
    const [url, options] = fetchSpy.mock.calls[0];
    expect(url).toContain('holds/h_1');
    expect(options?.method).toBe('DELETE');
  });
});

