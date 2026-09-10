import { useState } from 'react'
import { ApiError } from '@/api/errors'
import type { Equipment, Room } from '@/api/rooms'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { EquipmentSearchBox } from '@/features/rooms/components/EquipmentSearchBox'
import { useAssignEquipments, useRemoveEquipment } from '@/features/rooms/hooks/useRooms'

interface RoomEquipmentEditorProps {
  room: Room
  unassignedEquipments: Equipment[]
}

/**
 * Gestao de equipamentos de uma sala existente (spec 007), embutida na
 * propria tela de edicao — nao um dialogo a parte. Cada acao chama a API na
 * hora: PATCH /api/rooms/{id} nao toca em equipamentos
 * (docs/services/room-service.md), entao adicionar/remover aqui e a unica
 * porta, e nao ha "Salvar" para isso.
 */
export function RoomEquipmentEditor({ room, unassignedEquipments }: RoomEquipmentEditorProps) {
  const [error, setError] = useState<string | null>(null)
  const assign = useAssignEquipments()
  const remove = useRemoveEquipment()

  async function handleAssign(equipment: Equipment) {
    setError(null)
    try {
      await assign.mutateAsync({ roomId: room.id, equipmentIds: [equipment.id] })
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else {
        throw err
      }
    }
  }

  async function handleRemove(equipmentId: string) {
    setError(null)
    try {
      await remove.mutateAsync({ roomId: room.id, equipmentId })
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message)
      } else {
        throw err
      }
    }
  }

  return (
    <div className="mt-8 flex flex-col gap-4 border-t border-border pt-8">
      <div>
        <h2 className="font-display text-lg font-semibold tracking-tight">Equipamentos desta sala</h2>
        <p className="text-sm text-muted">Adicionar ou remover aqui já grava — não precisa salvar de novo.</p>
      </div>

      <div className="flex flex-col gap-2">
        <Label htmlFor="assign-equipment-search">Adicionar equipamento</Label>
        <EquipmentSearchBox
          id="assign-equipment-search"
          equipments={unassignedEquipments}
          onSelect={handleAssign}
          emptyMessage="Nenhum equipamento livre para adicionar."
        />
      </div>

      {error && (
        <p role="alert" className="text-sm text-danger">
          {error}
        </p>
      )}

      {room.equipments.length === 0 ? (
        <p className="text-sm text-muted">Esta sala não tem equipamentos.</p>
      ) : (
        <ul className="flex flex-col gap-2">
          {room.equipments.map((equipment) => (
            <li
              key={equipment.id}
              className="flex items-center justify-between gap-3 rounded-md border border-border bg-surface px-3 py-2 text-sm"
            >
              <span>
                {equipment.type} · {equipment.brand}{' '}
                <span className="text-muted">({equipment.serialNumber})</span>
              </span>
              <Button
                type="button"
                variant="ghost"
                size="sm"
                onClick={() => void handleRemove(equipment.id)}
              >
                Remover
                <span className="sr-only">
                  {' '}
                  {equipment.type} {equipment.serialNumber}
                </span>
              </Button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
