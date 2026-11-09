import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { holdsApi } from '../api/holds.api';
import { useStudentId } from '../../identity/hooks/useStudentId';
import { CLASSES_QUERY_KEY } from '../../admission-classes/hooks/useClasses';

export const MY_HOLDS_QUERY_KEY = ['my-holds'];

export function useMyHolds() {
  const { studentId, hasStudentId } = useStudentId();

  return useQuery({
    queryKey: [...MY_HOLDS_QUERY_KEY, studentId],
    queryFn: holdsApi.getMyHolds,
    enabled: hasStudentId,
    refetchInterval: (query) => {
      // Poll faster (every 3.5 seconds) if there's an active HELD application
      const hasActiveHeld = query.state.data?.some((h) => h.status === 'HELD');
      return hasActiveHeld ? 3500 : 8000;
    },
  });
}

export function useConfirmHold() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (holdId: string) => holdsApi.confirmHold(holdId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MY_HOLDS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: CLASSES_QUERY_KEY });
    },
  });
}

export function useCancelHold() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (holdId: string) => holdsApi.cancelHold(holdId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: MY_HOLDS_QUERY_KEY });
      queryClient.invalidateQueries({ queryKey: CLASSES_QUERY_KEY });
    },
  });
}
