import { useState } from 'react'
import type { Room } from '@/api/rooms'
import { EmptyState } from '@/components/EmptyState'
import { ErrorState } from '@/components/ErrorState'
import { PageHeader } from '@/components/PageHeader'
import { Skeleton } from '@/components/Skeleton'
import { Button } from '@/components/ui/button'
import { RoomForm } from '@/features/rooms/components/RoomForm'
import { RoomList } from '@/features/rooms/components/RoomList'
import {
  useCreateRoom,
  useRooms,
  useUnassignedEquipments,
  useUpdateRoom,
} from '@/features/rooms/hooks/useRooms'

type Mode = { kind: 'list' } | { kind: 'create' } | { kind: 'edit'; room: Room }

export function RoomsPage() {
  const [mode, setMode] = useState<Mode>({ kind: 'list' })
  const rooms = useRooms()
  const equipments = useUnassignedEquipments()
  const createRoom = useCreateRoom()
  const updateRoom = useUpdateRoom()

  if (mode.kind === 'create') {
    return (
      <>
        <PageHeader title="Nova sala" />
        <RoomForm
          equipments={equipments.data ?? []}
          submitLabel="Cadastrar sala"
          onCancel={() => setMode({ kind: 'list' })}
          onSubmit={async (values) => {
            await createRoom.mutateAsync({
              name: values.name,
              number: values.number,
              planSlot: values.planSlot,
              equipmentIds: values.equipmentIds,
            })
            setMode({ kind: 'list' })
          }}
        />
      </>
    )
  }

  if (mode.kind === 'edit') {
    return (
      <>
        <PageHeader title={`Editar ${mode.room.name}`} />
        <RoomForm
          room={mode.room}
          submitLabel="Salvar alterações"
          onCancel={() => setMode({ kind: 'list' })}
          onSubmit={async (values) => {
            await updateRoom.mutateAsync({
              id: mode.room.id,
              input: {
                name: values.name,
                number: values.number,
                ...(values.planSlot === null ? {} : { planSlot: values.planSlot }),
              },
            })
            setMode({ kind: 'list' })
          }}
        />
      </>
    )
  }

  const newRoomButton = <Button onClick={() => setMode({ kind: 'create' })}>Nova sala</Button>

  return (
    <>
      <PageHeader title="Salas" action={rooms.data && rooms.data.length > 0 ? newRoomButton : undefined} />

      {rooms.isPending && (
        <div className="flex flex-col gap-3" aria-busy="true" aria-label="Carregando salas">
          <Skeleton className="h-14" />
          <Skeleton className="h-14" />
          <Skeleton className="h-14" />
        </div>
      )}

      {rooms.isError && (
        <ErrorState
          description="Não foi possível carregar as salas."
          onRetry={() => void rooms.refetch()}
        />
      )}

      {rooms.isSuccess && rooms.data.length === 0 && (
        <EmptyState
          title="Nenhuma sala cadastrada"
          description="Cadastre a primeira sala para começar a reservar."
          action={newRoomButton}
        />
      )}

      {rooms.isSuccess && rooms.data.length > 0 && (
        <RoomList rooms={rooms.data} onEdit={(room) => setMode({ kind: 'edit', room })} />
      )}
    </>
  )
}
