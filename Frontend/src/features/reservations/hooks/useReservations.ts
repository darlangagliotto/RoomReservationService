import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/api'
import {
  cancelReservation,
  createReservation,
  listMyReservations,
  type CreateReservationInput,
} from '@/api/reservations'
import { useAuth } from '@/auth/useAuth'

const reservationsKey = (userId: string) => ['reservations', { userId }] as const

/**
 * Não há endpoint de "minhas reservas": o filtro é `userId` do claim `sub`
 * (spec 008, §5). Conveniência de tela, não segurança — o backend aceita
 * qualquer `userId` em quem estiver autenticado.
 */
export function useMyReservations() {
  const { session } = useAuth()
  const userId = session?.userId ?? null

  return useQuery({
    queryKey: reservationsKey(userId ?? ''),
    queryFn: () => listMyReservations(apiClient, userId!),
    enabled: userId !== null,
  })
}

export function useCreateReservation() {
  const { session } = useAuth()
  const queryClient = useQueryClient()
  const userId = session?.userId ?? null

  return useMutation({
    mutationFn: (input: Omit<CreateReservationInput, 'userId'>) => {
      if (userId === null) {
        throw new Error('Sessão sem userId — não deveria ser possível chegar aqui autenticado.')
      }
      return createReservation(apiClient, { ...input, userId })
    },
    onSuccess: async () => {
      if (userId !== null) {
        await queryClient.invalidateQueries({ queryKey: reservationsKey(userId) })
      }
      // Uma reserva nova muda o status que a Home mostra (spec 009).
      await queryClient.invalidateQueries({ queryKey: ['availability'] })
    },
  })
}

export function useCancelReservation() {
  const { session } = useAuth()
  const queryClient = useQueryClient()
  const userId = session?.userId ?? null

  return useMutation({
    mutationFn: (id: string) => cancelReservation(apiClient, id),
    onSuccess: async () => {
      if (userId !== null) {
        await queryClient.invalidateQueries({ queryKey: reservationsKey(userId) })
      }
      await queryClient.invalidateQueries({ queryKey: ['availability'] })
    },
  })
}
