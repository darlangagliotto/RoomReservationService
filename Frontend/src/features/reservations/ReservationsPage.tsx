import { useState } from 'react'
import { ApiError } from '@/api/errors'
import type { Reservation } from '@/api/reservations'
import { EmptyState } from '@/components/EmptyState'
import { ErrorState } from '@/components/ErrorState'
import { PageHeader } from '@/components/PageHeader'
import { Skeleton } from '@/components/Skeleton'
import { Button } from '@/components/ui/button'
import { Dialog, DialogClose, DialogContent, DialogDescription, DialogTitle } from '@/components/ui/dialog'
import { ReservationForm } from '@/features/reservations/components/ReservationForm'
import { ReservationList } from '@/features/reservations/components/ReservationList'
import {
  useCancelReservation,
  useCreateReservation,
  useMyReservations,
} from '@/features/reservations/hooks/useReservations'
import { useRooms } from '@/features/rooms/hooks/useRooms'
import { formatLocalDateTime } from '@/lib/datetime'

export function ReservationsPage() {
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [confirming, setConfirming] = useState<Reservation | null>(null)
  const [cancelError, setCancelError] = useState<string | null>(null)

  const reservations = useMyReservations()
  const rooms = useRooms()
  const createReservation = useCreateReservation()
  const cancelReservation = useCancelReservation()

  function closeConfirm() {
    setConfirming(null)
    setCancelError(null)
  }

  async function handleConfirmCancel() {
    if (!confirming) return
    setCancelError(null)
    try {
      await cancelReservation.mutateAsync(confirming.id)
      setConfirming(null)
    } catch (error) {
      if (error instanceof ApiError) {
        setCancelError(error.message)
      } else {
        throw error
      }
    }
  }

  const newReservationButton = (
    <Button onClick={() => setIsCreateOpen(true)}>Nova reserva</Button>
  )

  return (
    <>
      <PageHeader
        title="Reservas"
        action={reservations.data && reservations.data.length > 0 ? newReservationButton : undefined}
      />

      {reservations.isPending && (
        <div className="flex flex-col gap-3" aria-busy="true" aria-label="Carregando reservas">
          <Skeleton className="h-14" />
          <Skeleton className="h-14" />
          <Skeleton className="h-14" />
        </div>
      )}

      {reservations.isError && (
        <ErrorState
          description="Não foi possível carregar as reservas."
          onRetry={() => void reservations.refetch()}
        />
      )}

      {reservations.isSuccess && reservations.data.length === 0 && (
        <EmptyState
          title="Nenhuma reserva ainda"
          description="Reserve uma sala para vê-la aqui."
          action={newReservationButton}
        />
      )}

      {reservations.isSuccess && reservations.data.length > 0 && (
        <ReservationList
          reservations={reservations.data}
          onCancel={(reservation) => {
            setConfirming(reservation)
            setCancelError(null)
          }}
        />
      )}

      <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
        <DialogContent>
          <DialogTitle>Nova reserva</DialogTitle>
          <DialogDescription>Escolha a sala e o intervalo.</DialogDescription>
          <div className="mt-4">
            <ReservationForm
              rooms={rooms.data ?? []}
              onCancel={() => setIsCreateOpen(false)}
              onSubmit={async (values) => {
                await createReservation.mutateAsync(values)
                setIsCreateOpen(false)
              }}
            />
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={confirming !== null} onOpenChange={(open) => !open && closeConfirm()}>
        <DialogContent>
          <DialogTitle>Cancelar reserva?</DialogTitle>
          <DialogDescription>
            {confirming &&
              `${confirming.roomName}, ${formatLocalDateTime(confirming.startDate)} até ${formatLocalDateTime(confirming.endDate)}. Esta ação não pode ser desfeita.`}
          </DialogDescription>

          {cancelError && (
            <p role="alert" className="mt-3 text-sm text-danger">
              {cancelError}
            </p>
          )}

          <div className="mt-6 flex flex-wrap gap-3">
            <Button
              type="button"
              variant="ghost"
              className="text-danger hover:bg-danger/10"
              disabled={cancelReservation.isPending}
              aria-busy={cancelReservation.isPending}
              onClick={() => void handleConfirmCancel()}
            >
              {cancelReservation.isPending ? 'Cancelando…' : 'Cancelar reserva'}
            </Button>
            <DialogClose asChild>
              <Button type="button" variant="ghost">
                Voltar
              </Button>
            </DialogClose>
          </div>
        </DialogContent>
      </Dialog>
    </>
  )
}
