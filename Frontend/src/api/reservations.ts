/*
  Contrato do ReservationService. Ver docs/services/reservation-service.md.
*/

import type { ApiClient } from '@/api/client'

export interface Reservation {
  id: string
  userId: string
  userName: string
  roomId: string
  roomName: string
  roomNumber: number
  startDate: string
  endDate: string
}

export interface CreateReservationInput {
  userId: string
  roomId: string
  startDate: string
  endDate: string
}

export type RoomStatus = 'Disponivel' | 'Reservada' | 'EmUso'

export interface RoomAvailability {
  roomId: string
  roomName: string
  roomNumber: number
  status: RoomStatus
  busyUntil: string | null
  nextReservationAt: string | null
}

/**
 * Sempre devolve todas as salas, inclusive sem reserva (spec 003) — 200 com
 * [], nunca o 400 do ADR-011. Por isso `request`, não `requestList`.
 */
export function getAvailability(
  client: ApiClient,
  start: string,
  end: string,
): Promise<RoomAvailability[]> {
  return client.request<RoomAvailability[]>(
    `/api/reservations/availability?start=${encodeURIComponent(start)}&end=${encodeURIComponent(end)}`,
  )
}

/**
 * Resultado vazio devolve 400 "Nenhuma reserva encontrada." (ADR-011); por
 * isso `requestList`, que converte isso em lista vazia — "não tenho reserva
 * nenhuma" é estado vazio de tela, nunca erro.
 */
export function listMyReservations(client: ApiClient, userId: string): Promise<Reservation[]> {
  return client.requestList<Reservation>(`/api/reservations?userId=${userId}`)
}

export function createReservation(
  client: ApiClient,
  input: CreateReservationInput,
): Promise<{ reservation: Reservation }> {
  return client.request<{ reservation: Reservation }>('/api/reservations', {
    method: 'POST',
    body: input,
  })
}

export function cancelReservation(client: ApiClient, id: string): Promise<{ id: string }> {
  return client.request<{ id: string }>(`/api/reservations/${id}`, { method: 'DELETE' })
}
