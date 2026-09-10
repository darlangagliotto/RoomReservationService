/*
  Contrato do RoomService para equipamentos. Ver docs/services/room-service.md
  e docs/domain/model.md#equipment--raiz-roomservice.
*/

import type { ApiClient } from '@/api/client'

/**
 * Vocabulario controlado de EquipmentType. Mantido em sincronia com o enum do
 * dominio (docs/domain/model.md) — divergencia e recusada pelo servidor.
 */
export const EQUIPMENT_TYPES = [
  'Tv',
  'Monitor',
  'QuadroBranco',
  'Projetor',
  'ArCondicionado',
  'Telefone',
  'Notebook',
  'Dock',
  'Flipchart',
  'Cadeira',
  'Outro',
] as const

export type EquipmentType = (typeof EQUIPMENT_TYPES)[number]

export const EQUIPMENT_TYPE_LABELS: Record<EquipmentType, string> = {
  Tv: 'TV',
  Monitor: 'Monitor',
  QuadroBranco: 'Quadro branco',
  Projetor: 'Projetor',
  ArCondicionado: 'Ar-condicionado',
  Telefone: 'Telefone',
  Notebook: 'Notebook',
  Dock: 'Dock',
  Flipchart: 'Flipchart',
  Cadeira: 'Cadeira',
  Outro: 'Outro',
}

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

export interface CreateEquipmentInput {
  type: string
  brand: string
  serialNumber: string
  purchaseDate: string
}

export interface EquipmentFilters {
  type?: string
  unassignedOnly?: boolean
}

function buildQuery(filters: EquipmentFilters): string {
  const params = new URLSearchParams()
  if (filters.type) params.set('type', filters.type)
  if (filters.unassignedOnly) params.set('unassigned', 'true')
  const query = params.toString()
  return query ? `?${query}` : ''
}

/**
 * Excecao deliberada da spec 004: lista vazia devolve 200 com [], nao o 400
 * do ADR-011. Por isso `request`, nunca `requestList` — nao ha mensagem de
 * "nenhum equipamento encontrado" para interpretar aqui.
 */
export function listEquipments(
  client: ApiClient,
  filters: EquipmentFilters = {},
): Promise<Equipment[]> {
  return client.request<Equipment[]>(`/api/equipments${buildQuery(filters)}`)
}

export function createEquipment(
  client: ApiClient,
  input: CreateEquipmentInput,
): Promise<{ equipment: Equipment }> {
  return client.request<{ equipment: Equipment }>('/api/equipments', {
    method: 'POST',
    body: input,
  })
}
