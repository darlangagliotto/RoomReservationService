import type { RoomAvailability } from '@/api/reservations'
import type { Room } from '@/api/rooms'
import { Button } from '@/components/ui/button'
import { Sheet, SheetContent, SheetDescription, SheetTitle } from '@/components/ui/sheet'
import { Skeleton } from '@/components/Skeleton'
import { describeStatus } from '@/features/home/roomStatus'
import { formatLocalDateTime } from '@/lib/datetime'

interface RoomDetailPanelProps {
  availability: RoomAvailability | undefined
  /** Dados completos (equipamentos) — pode ainda não ter chegado quando o painel já abriu. */
  room: Room | undefined
  onClose: () => void
  onReserve: () => void
}

function contextLine(availability: RoomAvailability): string | null {
  if (availability.status === 'EmUso' && availability.busyUntil) {
    return `Ocupada até ${formatLocalDateTime(availability.busyUntil)}`
  }
  if (availability.status === 'Reservada' && availability.nextReservationAt) {
    return `Próxima reserva às ${formatLocalDateTime(availability.nextReservationAt)}`
  }
  return null
}

/**
 * Painel lateral de detalhe (spec 009): responde "o que tem nesta sala,
 * está livre, e como eu reservo" sem sair da grade de cards.
 */
export function RoomDetailPanel({ availability, room, onClose, onReserve }: RoomDetailPanelProps) {
  const status = availability ? describeStatus(availability.status) : null
  const context = availability ? contextLine(availability) : null

  return (
    <Sheet open={availability !== undefined} onOpenChange={(open) => !open && onClose()}>
      <SheetContent>
        {availability && (
          <>
            <SheetTitle>{availability.roomName}</SheetTitle>
            <SheetDescription className="font-data">nº {availability.roomNumber}</SheetDescription>

            <div className="mt-4 flex items-center gap-2 text-sm">
              <span aria-hidden className="size-2 rounded-full" style={{ backgroundColor: status?.colorVar }} />
              <span>{status?.label}</span>
              {context && <span className="text-muted">· {context}</span>}
            </div>

            <div className="mt-6 flex flex-col gap-2">
              <h3 className="text-sm font-medium">Equipamentos</h3>
              {room === undefined ? (
                <div className="flex flex-col gap-2" aria-busy="true" aria-label="Carregando equipamentos">
                  <Skeleton className="h-8" />
                  <Skeleton className="h-8" />
                </div>
              ) : room.equipments.length === 0 ? (
                <p className="text-sm text-muted">Sem equipamentos.</p>
              ) : (
                <ul className="flex flex-col gap-2">
                  {room.equipments.map((equipment) => (
                    <li
                      key={equipment.id}
                      className="rounded-md border border-border bg-background px-3 py-2 text-sm"
                    >
                      {equipment.type} · {equipment.brand}{' '}
                      <span className="text-muted">({equipment.serialNumber})</span>
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <div className="mt-auto pt-6">
              <Button onClick={onReserve}>Reservar esta sala</Button>
            </div>
          </>
        )}
      </SheetContent>
    </Sheet>
  )
}
