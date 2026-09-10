import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/api'
import {
  assignEquipmentsToRoom,
  createRoom,
  listRooms,
  listUnassignedEquipments,
  removeEquipmentFromRoom,
  updateRoom,
  type CreateRoomInput,
  type UpdateRoomInput,
} from '@/api/rooms'

const roomsKey = ['rooms'] as const

export function useRooms() {
  return useQuery({ queryKey: roomsKey, queryFn: () => listRooms(apiClient) })
}

export function useUnassignedEquipments() {
  return useQuery({
    queryKey: ['equipments', { unassigned: true }],
    queryFn: () => listUnassignedEquipments(apiClient),
  })
}

export function useCreateRoom() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (input: CreateRoomInput) => createRoom(apiClient, input),
    // Sem atualizacao otimista: unicidade de nome, numero e planSlot so o
    // servidor sabe. Ver skill frontend-app.
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: roomsKey })
      await queryClient.invalidateQueries({ queryKey: ['equipments'] })
    },
  })
}

export function useUpdateRoom() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, input }: { id: string; input: UpdateRoomInput }) =>
      updateRoom(apiClient, id, input),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: roomsKey })
    },
  })
}

export function useAssignEquipments() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ roomId, equipmentIds }: { roomId: string; equipmentIds: string[] }) =>
      assignEquipmentsToRoom(apiClient, roomId, equipmentIds),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: roomsKey })
      await queryClient.invalidateQueries({ queryKey: ['equipments'] })
    },
  })
}

export function useRemoveEquipment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ roomId, equipmentId }: { roomId: string; equipmentId: string }) =>
      removeEquipmentFromRoom(apiClient, roomId, equipmentId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: roomsKey })
      await queryClient.invalidateQueries({ queryKey: ['equipments'] })
    },
  })
}
