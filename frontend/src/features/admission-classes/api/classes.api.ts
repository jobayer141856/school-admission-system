import { apiClient } from '../../../lib/api-client';
import { AdmissionClassItem, CreateClassPayload, AdmissionClassCreated } from '../types/class.types';

export const classesApi = {
  getClasses: () => apiClient<AdmissionClassItem[]>('classes'),
  getClassById: (classId: string) => apiClient<AdmissionClassItem>(`classes/${classId}`),
  createClass: (payload: CreateClassPayload) =>
    apiClient<AdmissionClassCreated>('classes', {
      method: 'POST',
      body: JSON.stringify(payload),
    }),
};

