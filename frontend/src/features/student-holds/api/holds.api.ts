import { apiClient } from '../../../lib/api-client';
import { StudentHoldItem, ConfirmHoldResponse } from '../types/hold.types';

export const holdsApi = {
  getMyHolds: () => apiClient<StudentHoldItem[]>('me/holds'),

  confirmHold: (holdId: string) =>
    apiClient<ConfirmHoldResponse>(`holds/${holdId}/confirm`, {
      method: 'POST',
    }),

  cancelHold: (holdId: string) =>
    apiClient<void>(`holds/${holdId}`, {
      method: 'DELETE',
    }),
};
