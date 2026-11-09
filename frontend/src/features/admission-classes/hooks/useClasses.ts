import { useQuery } from '@tanstack/react-query';
import { classesApi } from '../api/classes.api';

export const CLASSES_QUERY_KEY = ['admission-classes'];

export function useClasses() {
  return useQuery({
    queryKey: CLASSES_QUERY_KEY,
    queryFn: classesApi.getClasses,
    refetchInterval: 5000, // keep seat counts fresh
  });
}

