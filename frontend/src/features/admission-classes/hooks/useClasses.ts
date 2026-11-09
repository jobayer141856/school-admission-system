import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { classesApi } from '../api/classes.api';
import { CreateClassPayload } from '../types/class.types';

export const CLASSES_QUERY_KEY = ['admission-classes'];

export function useClasses() {
  return useQuery({
    queryKey: CLASSES_QUERY_KEY,
    queryFn: classesApi.getClasses,
    refetchInterval: 5000, // keep seat counts fresh
  });
}

export function useCreateClass() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateClassPayload) => classesApi.createClass(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CLASSES_QUERY_KEY });
    },
  });
}
