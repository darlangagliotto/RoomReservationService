import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/api'
import {
  createEquipment,
  listEquipments,
  type CreateEquipmentInput,
  type EquipmentFilters,
} from '@/api/equipments'

export function useEquipments(filters: EquipmentFilters) {
  return useQuery({
    queryKey: ['equipments', filters],
    queryFn: () => listEquipments(apiClient, filters),
  })
}

export function useCreateEquipment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: CreateEquipmentInput) => createEquipment(apiClient, input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ['equipments'] })
    },
  })
}
