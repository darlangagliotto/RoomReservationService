import type { Room } from '@/api/rooms'
import { Button } from '@/components/ui/button'
import { useIsWideScreen } from '@/lib/useMediaQuery'

interface RoomListProps {
  rooms: Room[]
  onEdit: (room: Room) => void
}

/**
 * Mesma informacao em duas apresentacoes: tabela em tela larga, cartoes em
 * tela estreita. Nao e degradacao — e a apresentacao certa para cada espaco.
 * So uma das duas e renderizada (ver useMediaQuery).
 */
export function RoomList({ rooms, onEdit }: RoomListProps) {
  return useIsWideScreen() ? (
    <RoomTable rooms={rooms} onEdit={onEdit} />
  ) : (
    <RoomCards rooms={rooms} onEdit={onEdit} />
  )
}

function RoomTable({ rooms, onEdit }: RoomListProps) {
  return (
    <div className="overflow-x-auto rounded-lg border border-border">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-border bg-surface">
          <tr>
            <th scope="col" className="px-4 py-3 font-medium">Nome</th>
            <th scope="col" className="px-4 py-3 font-medium">Número</th>
            <th scope="col" className="px-4 py-3 font-medium">Planta</th>
            <th scope="col" className="px-4 py-3 font-medium">Equipamentos</th>
            <th scope="col" className="px-4 py-3 font-medium">
              <span className="sr-only">Ações</span>
            </th>
          </tr>
        </thead>
        <tbody>
          {rooms.map((room) => (
            <tr key={room.id} className="border-b border-border last:border-0">
              <td className="px-4 py-3">{room.name}</td>
              <td className="px-4 py-3 font-data">{room.number}</td>
              <td className="px-4 py-3 font-data text-muted">{room.planSlot ?? '—'}</td>
              <td className="px-4 py-3 text-muted">{describeEquipments(room)}</td>
              <td className="px-4 py-3 text-right">
                <Button variant="ghost" size="sm" onClick={() => onEdit(room)}>
                  Editar<span className="sr-only"> {room.name}</span>
                </Button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function RoomCards({ rooms, onEdit }: RoomListProps) {
  return (
    <ul className="flex flex-col gap-3">
      {rooms.map((room) => (
        <li key={room.id} className="rounded-lg border border-border bg-surface p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="font-medium">{room.name}</p>
              <p className="text-sm text-muted">
                <span className="font-data">nº {room.number}</span>
                {' · '}planta {room.planSlot ?? '—'}
              </p>
            </div>
            <Button variant="ghost" size="sm" onClick={() => onEdit(room)}>
              Editar<span className="sr-only"> {room.name}</span>
            </Button>
          </div>
          <p className="mt-3 text-sm text-muted">{describeEquipments(room)}</p>
        </li>
      ))}
    </ul>
  )
}

function describeEquipments(room: Room): string {
  if (room.equipments.length === 0) {
    return 'Sem equipamentos'
  }

  return room.equipments.map((e) => `${e.type} (${e.placement.toLowerCase()})`).join(', ')
}
