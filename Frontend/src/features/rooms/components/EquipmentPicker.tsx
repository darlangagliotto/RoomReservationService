import type { Equipment } from '@/api/rooms'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { EquipmentSearchBox } from '@/features/rooms/components/EquipmentSearchBox'

interface EquipmentPickerProps {
  equipments: Equipment[]
  selectedIds: string[]
  onChange: (ids: string[]) => void
}

function describe(equipment: Equipment): string {
  return `${equipment.type} · ${equipment.brand} (${equipment.serialNumber})`
}

/**
 * Selecao local (nao persistida) para o formulario de cadastro de sala: o
 * equipamento so e alocado de fato quando a sala e criada (POST /api/rooms
 * com equipmentIds). Ver EquipmentSearchBox para a busca em si.
 */
export function EquipmentPicker({ equipments, selectedIds, onChange }: EquipmentPickerProps) {
  const selected = selectedIds
    .map((id) => equipments.find((e) => e.id === id))
    .filter((e): e is Equipment => e !== undefined)

  const candidates = equipments.filter((e) => !selectedIds.includes(e.id))

  function remove(id: string) {
    onChange(selectedIds.filter((existing) => existing !== id))
  }

  return (
    <div className="flex flex-col gap-2">
      <Label htmlFor="equipment-search">Equipamentos disponíveis</Label>

      <EquipmentSearchBox
        id="equipment-search"
        equipments={candidates}
        onSelect={(equipment) => onChange([...selectedIds, equipment.id])}
        emptyMessage="Nenhum equipamento livre. Equipamento já alocado a outra sala não pode ser reutilizado."
      />

      {selected.length > 0 && (
        <ul className="mt-1 flex flex-col gap-2">
          {selected.map((equipment) => (
            <li
              key={equipment.id}
              className="flex items-center justify-between gap-3 rounded-md border border-border bg-surface px-3 py-2 text-sm"
            >
              <span>{describe(equipment)}</span>
              <Button type="button" variant="ghost" size="sm" onClick={() => remove(equipment.id)}>
                Remover<span className="sr-only"> {describe(equipment)}</span>
              </Button>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
