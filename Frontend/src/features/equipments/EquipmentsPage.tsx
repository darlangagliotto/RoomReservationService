import { useMemo, useState } from 'react'
import { EQUIPMENT_TYPE_LABELS, EQUIPMENT_TYPES } from '@/api/equipments'
import { EmptyState } from '@/components/EmptyState'
import { ErrorState } from '@/components/ErrorState'
import { PageHeader } from '@/components/PageHeader'
import { Skeleton } from '@/components/Skeleton'
import { Button } from '@/components/ui/button'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { EquipmentForm } from '@/features/equipments/components/EquipmentForm'
import { EquipmentList } from '@/features/equipments/components/EquipmentList'
import { useCreateEquipment, useEquipments } from '@/features/equipments/hooks/useEquipments'
import { useRooms } from '@/features/rooms/hooks/useRooms'

type Mode = { kind: 'list' } | { kind: 'create' }

export function EquipmentsPage() {
  const [mode, setMode] = useState<Mode>({ kind: 'list' })
  const [type, setType] = useState('')
  const [unassignedOnly, setUnassignedOnly] = useState(false)

  const equipments = useEquipments({ type: type || undefined, unassignedOnly })
  // So para resolver o nome da sala na listagem — nenhum endpoint novo.
  const rooms = useRooms()
  const createEquipment = useCreateEquipment()

  const roomNameById = useMemo(() => {
    const map = new Map<string, string>()
    for (const room of rooms.data ?? []) map.set(room.id, room.name)
    return map
  }, [rooms.data])

  const hasActiveFilter = type !== '' || unassignedOnly

  if (mode.kind === 'create') {
    return (
      <>
        <PageHeader title="Novo equipamento" />
        <EquipmentForm
          submitLabel="Cadastrar equipamento"
          onCancel={() => setMode({ kind: 'list' })}
          onSubmit={async (values) => {
            await createEquipment.mutateAsync(values)
            setMode({ kind: 'list' })
          }}
        />
      </>
    )
  }

  const newEquipmentButton = (
    <Button onClick={() => setMode({ kind: 'create' })}>Novo equipamento</Button>
  )

  return (
    <>
      <PageHeader
        title="Equipamentos"
        action={equipments.data && equipments.data.length > 0 ? newEquipmentButton : undefined}
      />

      <div className="mb-6 flex flex-wrap items-end gap-4">
        <div className="flex flex-col gap-2">
          <Label htmlFor="type-filter">Tipo</Label>
          <Select
            id="type-filter"
            className="w-auto min-w-48"
            value={type}
            onChange={(event) => setType(event.target.value)}
          >
            <option value="">Todos os tipos</option>
            {EQUIPMENT_TYPES.map((t) => (
              <option key={t} value={t}>
                {EQUIPMENT_TYPE_LABELS[t]}
              </option>
            ))}
          </Select>
        </div>

        <label className="flex h-11 items-center gap-2 text-sm">
          <input
            type="checkbox"
            checked={unassignedOnly}
            onChange={(event) => setUnassignedOnly(event.target.checked)}
            className="size-4 accent-[var(--color-accent)]"
          />
          Apenas livres
        </label>
      </div>

      {equipments.isPending && (
        <div className="flex flex-col gap-3" aria-busy="true" aria-label="Carregando equipamentos">
          <Skeleton className="h-14" />
          <Skeleton className="h-14" />
          <Skeleton className="h-14" />
        </div>
      )}

      {equipments.isError && (
        <ErrorState
          description="Não foi possível carregar os equipamentos."
          onRetry={() => void equipments.refetch()}
        />
      )}

      {equipments.isSuccess && equipments.data.length === 0 && !hasActiveFilter && (
        <EmptyState
          title="Nenhum equipamento cadastrado"
          description="Cadastre o primeiro equipamento para começar a montar o patrimônio."
          action={newEquipmentButton}
        />
      )}

      {equipments.isSuccess && equipments.data.length === 0 && hasActiveFilter && (
        <EmptyState
          title="Nenhum equipamento encontrado"
          description="Nenhum equipamento corresponde ao filtro atual."
          action={newEquipmentButton}
        />
      )}

      {equipments.isSuccess && equipments.data.length > 0 && (
        <EquipmentList equipments={equipments.data} roomNameById={roomNameById} />
      )}
    </>
  )
}
