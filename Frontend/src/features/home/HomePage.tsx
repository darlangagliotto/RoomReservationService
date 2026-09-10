import { useState } from 'react'
import { EmptyState } from '@/components/EmptyState'
import { ErrorState } from '@/components/ErrorState'
import { PageHeader } from '@/components/PageHeader'
import { Skeleton } from '@/components/Skeleton'
import { Dialog, DialogContent, DialogDescription, DialogTitle } from '@/components/ui/dialog'
import { RoomDetailPanel } from '@/features/home/components/RoomDetailPanel'
import { RoomStatusCard } from '@/features/home/components/RoomStatusCard'
import { useAvailability } from '@/features/home/hooks/useAvailability'
import { ReservationForm } from '@/features/reservations/components/ReservationForm'
import { useCreateReservation } from '@/features/reservations/hooks/useReservations'
import { useRooms } from '@/features/rooms/hooks/useRooms'

/**
 * Planta do andar (spec 009): grade de cards, um por sala, com estado agora
 * — não mais um mapa único do andar (ver information-architecture.md,
 * "Histórico da decisão visual").
 */
export function HomePage() {
  const [selectedRoomId, setSelectedRoomId] = useState<string | null>(null)
  const [reservingRoomId, setReservingRoomId] = useState<string | null>(null)

  const availability = useAvailability()
  const rooms = useRooms()
  const createReservation = useCreateReservation()

  const selectedAvailability = availability.data?.find((room) => room.roomId === selectedRoomId)
  const selectedRoom = rooms.data?.find((room) => room.id === selectedRoomId)

  return (
    <>
      <PageHeader title="Salas" />

      {availability.isPending && (
        <div
          className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3"
          aria-busy="true"
          aria-label="Carregando salas"
        >
          <Skeleton className="aspect-[4/3]" />
          <Skeleton className="aspect-[4/3]" />
          <Skeleton className="aspect-[4/3]" />
        </div>
      )}

      {availability.isError && (
        <ErrorState
          description="Não foi possível carregar as salas."
          onRetry={() => void availability.refetch()}
        />
      )}

      {availability.isSuccess && availability.data.length === 0 && (
        <EmptyState
          title="Nenhuma sala cadastrada"
          description="Cadastre uma sala para começar a reservar."
        />
      )}

      {availability.isSuccess && availability.data.length > 0 && (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {availability.data.map((room) => (
            <RoomStatusCard
              key={room.roomId}
              availability={room}
              onSelect={() => setSelectedRoomId(room.roomId)}
            />
          ))}
        </div>
      )}

      <RoomDetailPanel
        availability={selectedAvailability}
        room={selectedRoom}
        onClose={() => setSelectedRoomId(null)}
        onReserve={() => {
          setReservingRoomId(selectedRoomId)
          setSelectedRoomId(null)
        }}
      />

      <Dialog open={reservingRoomId !== null} onOpenChange={(open) => !open && setReservingRoomId(null)}>
        <DialogContent>
          <DialogTitle>Nova reserva</DialogTitle>
          <DialogDescription>Escolha o intervalo.</DialogDescription>
          <div className="mt-4">
            <ReservationForm
              rooms={rooms.data ?? []}
              initialRoomId={reservingRoomId ?? undefined}
              onCancel={() => setReservingRoomId(null)}
              onSubmit={async (values) => {
                await createReservation.mutateAsync(values)
                setReservingRoomId(null)
              }}
            />
          </div>
        </DialogContent>
      </Dialog>
    </>
  )
}
