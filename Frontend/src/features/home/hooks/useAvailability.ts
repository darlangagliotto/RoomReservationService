import { useMemo } from 'react'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/api'
import { getAvailability } from '@/api/reservations'
import { nowWindow } from '@/lib/datetime'

/**
 * Janela de referência fixa em "agora" (spec 009, §5) — sem slider nesta
 * versão. Calculada uma vez por montagem do componente: se recalculássemos a
 * cada render, `nowWindow()` mudaria a cada milissegundo, trocando a
 * `queryKey` e disparando um refetch infinito em vez de assentar.
 */
export function useAvailability() {
  const { start, end } = useMemo(() => nowWindow(), [])

  return useQuery({
    queryKey: ['availability', { start, end }],
    queryFn: () => getAvailability(apiClient, start, end),
  })
}
