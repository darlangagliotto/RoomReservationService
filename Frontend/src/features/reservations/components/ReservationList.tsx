import type { Reservation } from '@/api/reservations'
import { Button } from '@/components/ui/button'
import { formatLocalDateTime } from '@/lib/datetime'
import { useIsWideScreen } from '@/lib/useMediaQuery'

interface ReservationListProps {
  reservations: Reservation[]
  onCancel: (reservation: Reservation) => void
}

function isStarted(reservation: Reservation): boolean {
  return new Date(reservation.startDate).getTime() <= Date.now()
}

function sortByStart(reservations: Reservation[]): Reservation[] {
  return [...reservations].sort(
    (a, b) => new Date(a.startDate).getTime() - new Date(b.startDate).getTime(),
  )
}

/**
 * Mesma informacao em duas apresentacoes: tabela em tela larga, cartoes em
 * tela estreita. So uma das duas e renderizada (ver useMediaQuery).
 */
export function ReservationList({ reservations, onCancel }: ReservationListProps) {
  const sorted = sortByStart(reservations)

  return useIsWideScreen() ? (
    <ReservationTable reservations={sorted} onCancel={onCancel} />
  ) : (
    <ReservationCards reservations={sorted} onCancel={onCancel} />
  )
}

function ReservationTable({ reservations, onCancel }: ReservationListProps) {
  return (
    <div className="overflow-x-auto rounded-lg border border-border">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-border bg-surface">
          <tr>
            <th scope="col" className="px-4 py-3 font-medium">Sala</th>
            <th scope="col" className="px-4 py-3 font-medium">Número</th>
            <th scope="col" className="px-4 py-3 font-medium">Início</th>
            <th scope="col" className="px-4 py-3 font-medium">Fim</th>
            <th scope="col" className="px-4 py-3 font-medium">
              <span className="sr-only">Ações</span>
            </th>
          </tr>
        </thead>
        <tbody>
          {reservations.map((reservation) => (
            <tr key={reservation.id} className="border-b border-border last:border-0">
              <td className="px-4 py-3">{reservation.roomName}</td>
              <td className="px-4 py-3 font-data">{reservation.roomNumber}</td>
              <td className="px-4 py-3 font-data">{formatLocalDateTime(reservation.startDate)}</td>
              <td className="px-4 py-3 font-data">{formatLocalDateTime(reservation.endDate)}</td>
              <td className="px-4 py-3 text-right">
                {!isStarted(reservation) && (
                  <Button variant="ghost" size="sm" onClick={() => onCancel(reservation)}>
                    Cancelar<span className="sr-only"> reserva de {reservation.roomName}</span>
                  </Button>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function ReservationCards({ reservations, onCancel }: ReservationListProps) {
  return (
    <ul className="flex flex-col gap-3">
      {reservations.map((reservation) => (
        <li key={reservation.id} className="rounded-lg border border-border bg-surface p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="font-medium">{reservation.roomName}</p>
              <p className="text-sm text-muted">
                <span className="font-data">nº {reservation.roomNumber}</span>
              </p>
            </div>
            {!isStarted(reservation) && (
              <Button variant="ghost" size="sm" onClick={() => onCancel(reservation)}>
                Cancelar<span className="sr-only"> reserva de {reservation.roomName}</span>
              </Button>
            )}
          </div>
          <p className="mt-3 text-sm text-muted">
            {formatLocalDateTime(reservation.startDate)} até {formatLocalDateTime(reservation.endDate)}
          </p>
        </li>
      ))}
    </ul>
  )
}
