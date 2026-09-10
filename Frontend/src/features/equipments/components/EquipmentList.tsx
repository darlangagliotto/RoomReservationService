import type { Equipment } from '@/api/equipments'
import { useIsWideScreen } from '@/lib/useMediaQuery'
import { formatDate } from '@/lib/dates'

interface EquipmentListProps {
  equipments: Equipment[]
  roomNameById: ReadonlyMap<string, string>
}

function roomLabel(equipment: Equipment, roomNameById: ReadonlyMap<string, string>): string {
  if (equipment.roomId === null) return 'Livre'
  return roomNameById.get(equipment.roomId) ?? 'Livre'
}

/**
 * Mesma informacao em duas apresentacoes: tabela em tela larga, cartoes em
 * tela estreita. So uma das duas e renderizada (ver useMediaQuery).
 */
export function EquipmentList({ equipments, roomNameById }: EquipmentListProps) {
  return useIsWideScreen() ? (
    <EquipmentTable equipments={equipments} roomNameById={roomNameById} />
  ) : (
    <EquipmentCards equipments={equipments} roomNameById={roomNameById} />
  )
}

function EquipmentTable({ equipments, roomNameById }: EquipmentListProps) {
  return (
    <div className="overflow-x-auto rounded-lg border border-border">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-border bg-surface">
          <tr>
            <th scope="col" className="px-4 py-3 font-medium">Tipo</th>
            <th scope="col" className="px-4 py-3 font-medium">Âncora</th>
            <th scope="col" className="px-4 py-3 font-medium">Marca</th>
            <th scope="col" className="px-4 py-3 font-medium">Número de série</th>
            <th scope="col" className="px-4 py-3 font-medium">Data de compra</th>
            <th scope="col" className="px-4 py-3 font-medium">Sala</th>
          </tr>
        </thead>
        <tbody>
          {equipments.map((equipment) => (
            <tr key={equipment.id} className="border-b border-border last:border-0">
              <td className="px-4 py-3">{equipment.type}</td>
              <td className="px-4 py-3 text-muted">{equipment.placement}</td>
              <td className="px-4 py-3">{equipment.brand}</td>
              <td className="px-4 py-3 font-data">{equipment.serialNumber}</td>
              <td className="px-4 py-3 font-data text-muted">{formatDate(equipment.purchaseDate)}</td>
              <td className="px-4 py-3 text-muted">{roomLabel(equipment, roomNameById)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

function EquipmentCards({ equipments, roomNameById }: EquipmentListProps) {
  return (
    <ul className="flex flex-col gap-3">
      {equipments.map((equipment) => (
        <li key={equipment.id} className="rounded-lg border border-border bg-surface p-4">
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="font-medium">{equipment.type}</p>
              <p className="text-sm text-muted">
                {equipment.brand} · <span className="font-data">{equipment.serialNumber}</span>
              </p>
            </div>
            <span className="text-sm text-muted">{roomLabel(equipment, roomNameById)}</span>
          </div>
          <p className="mt-3 text-sm text-muted">
            Âncora: {equipment.placement} · Comprado em {formatDate(equipment.purchaseDate)}
          </p>
        </li>
      ))}
    </ul>
  )
}
