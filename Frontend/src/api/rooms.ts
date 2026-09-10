/*
  Contrato do RoomService. Ver docs/services/room-service.md.
*/

import type { ApiClient } from '@/api/client'
import { listEquipments, type Equipment } from '@/api/equipments'

export type { Equipment }

export interface Room {
  id: string
  name: string
  number: number
  /** Poligono da planta fixa; nulo = sala fora da planta. */
  planSlot: number | null
  equipments: Equipment[]
}

export interface CreateRoomInput {
  name: string
  number: number
  equipmentIds: string[]
  planSlot: number | null
}

export interface UpdateRoomInput {
  name?: string
  number?: number
  planSlot?: number
}

/** Busca sem resultado responde 400; requestList converte em lista vazia. */
export function listRooms(client: ApiClient): Promise<Room[]> {
  return client.requestList<Room>('/api/rooms')
}

export function createRoom(client: ApiClient, input: CreateRoomInput): Promise<{ room: Room }> {
  return client.request<{ room: Room }>('/api/rooms', { method: 'POST', body: input })
}

export function updateRoom(
  client: ApiClient,
  id: string,
  input: UpdateRoomInput,
): Promise<{ room: Room }> {
  return client.request<{ room: Room }>(`/api/rooms/${id}`, { method: 'PATCH', body: input })
}

export function listUnassignedEquipments(client: ApiClient): Promise<Equipment[]> {
  // Equipamento so pode estar numa sala: o seletor de cadastro nunca deve
  // oferecer um ja alocado.
  return listEquipments(client, { unassignedOnly: true })
}

/** Aloca equipamentos livres a uma sala existente. Ver docs/specs/007. */
export function assignEquipmentsToRoom(
  client: ApiClient,
  roomId: string,
  equipmentIds: string[],
): Promise<{ room: Room }> {
  return client.request<{ room: Room }>(`/api/rooms/${roomId}/equipments`, {
    method: 'POST',
    body: { equipmentIds },
  })
}

/** Desaloca um equipamento; ele volta a aparecer como livre. */
export function removeEquipmentFromRoom(
  client: ApiClient,
  roomId: string,
  equipmentId: string,
): Promise<{ room: Room }> {
  return client.request<{ room: Room }>(`/api/rooms/${roomId}/equipments/${equipmentId}`, {
    method: 'DELETE',
  })
}
