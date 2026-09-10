import type { RoomAvailability } from '@/api/reservations'
import { describeStatus } from '@/features/home/roomStatus'
import { pickRoomImage } from '@/lib/roomImages'

interface RoomStatusCardProps {
  availability: RoomAvailability
  onSelect: () => void
}

/**
 * Card de sala: imagem ilustrativa de fundo, nome, número e bolinha de
 * estado com texto — nunca a imagem inteira tingida pela cor do status
 * (spec 009, §5). É um <button>, não um <div> com onClick: navegação e
 * ativação por teclado vêm de graça.
 */
export function RoomStatusCard({ availability, onSelect }: RoomStatusCardProps) {
  const { label, colorVar } = describeStatus(availability.status)

  return (
    <button
      type="button"
      onClick={onSelect}
      className="group relative aspect-[4/3] w-full overflow-hidden rounded-lg border border-border text-left"
    >
      <img
        src={pickRoomImage(availability.roomId)}
        alt=""
        className="absolute inset-0 h-full w-full object-cover transition-transform group-hover:scale-105"
      />
      <div className="absolute inset-0 bg-gradient-to-t from-black/75 via-black/10 to-transparent" />

      <span className="absolute top-3 right-3 flex items-center gap-1.5 rounded-full bg-black/55 px-2.5 py-1 text-xs font-medium text-white">
        <span aria-hidden className="size-2 rounded-full" style={{ backgroundColor: colorVar }} />
        {label}
      </span>

      <span className="absolute inset-x-0 bottom-0 p-4">
        <span className="block font-display text-base font-bold text-white">
          {availability.roomName}
        </span>
        <span className="block font-data text-sm text-white/80">nº {availability.roomNumber}</span>
      </span>
    </button>
  )
}
