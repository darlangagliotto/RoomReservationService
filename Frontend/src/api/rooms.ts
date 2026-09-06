/*
  Contrato do RoomService. Ver docs/services/room-service.md.
*/

import type { ApiClient } from '@/api/client'

export interface Equipment {
  id: string
  type: string
  /** Ancora efetiva, ja resolvida pelo backend. O cliente nao deriva isso. */
  placement: string
  brand: string
  serialNumber: string
  purchaseDate: string
  roomId: string | null
}

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
  return client.requestList<Equipment>('/api/equipments?unassigned=true')
}
