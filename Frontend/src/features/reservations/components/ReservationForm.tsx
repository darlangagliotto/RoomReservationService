import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { ApiError } from '@/api/errors'
import type { Room } from '@/api/rooms'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { defaultReservationWindow, toApiUtc } from '@/lib/datetime'

/*
  Espelha as invariantes de docs/domain/model.md#reservation--raiz-reservationservice.
  Espelhar nao e substituir: sobreposicao continua sendo decidida so pelo
  servidor (spec 008, §5) — o cliente so evita a ida obvia ao servidor.
*/
const reservationSchema = z
  .object({
    roomId: z.string().min(1, 'Selecione uma sala.'),
    startDate: z.string().min(1, 'Informe o início.'),
    endDate: z.string().min(1, 'Informe o fim.'),
  })
  .refine((data) => new Date(data.startDate).getTime() > Date.now(), {
    message: 'O horário de início precisa estar no futuro.',
    path: ['startDate'],
  })
  .refine((data) => new Date(data.endDate).getTime() > new Date(data.startDate).getTime(), {
    message: 'O término precisa ser depois do início.',
    path: ['endDate'],
  })

type ParsedReservation = z.output<typeof reservationSchema>

export interface ReservationFormSubmit {
  roomId: string
  startDate: string
  endDate: string
}

interface ReservationFormProps {
  rooms: Room[]
  /** Entrada opcional pré-preenchida — a mesma peça é reaberta pela planta na spec 009. */
  initialRoomId?: string
  initialStart?: string
  initialEnd?: string
  onCancel: () => void
  onSubmit: (values: ReservationFormSubmit) => Promise<void>
}

export function ReservationForm({
  rooms,
  initialRoomId,
  initialStart,
  initialEnd,
  onCancel,
  onSubmit,
}: ReservationFormProps) {
  const suggested = defaultReservationWindow()
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(reservationSchema),
    defaultValues: {
      roomId: initialRoomId ?? '',
      startDate: initialStart ?? suggested.start,
      endDate: initialEnd ?? suggested.end,
    },
  })

  async function submit(values: ParsedReservation) {
    try {
      await onSubmit({
        roomId: values.roomId,
        startDate: toApiUtc(values.startDate),
        endDate: toApiUtc(values.endDate),
      })
    } catch (error) {
      if (error instanceof ApiError) {
        setError('root', { message: error.message })
      } else {
        throw error
      }
    }
  }

  return (
    <form onSubmit={handleSubmit(submit)} noValidate className="flex flex-col gap-5">
      <div className="flex flex-col gap-2">
        <Label htmlFor="roomId">Sala</Label>
        <Select
          id="roomId"
          autoFocus
          aria-invalid={errors.roomId !== undefined}
          aria-describedby={errors.roomId ? 'roomId-error' : undefined}
          {...register('roomId')}
        >
          <option value="">Selecione…</option>
          {rooms.map((room) => (
            <option key={room.id} value={room.id}>
              {room.name} (nº {room.number})
            </option>
          ))}
        </Select>
        {errors.roomId && (
          <p id="roomId-error" role="alert" className="text-sm text-danger">
            {errors.roomId.message}
          </p>
        )}
      </div>

      <div className="grid gap-5 sm:grid-cols-2">
        <div className="flex flex-col gap-2">
          <Label htmlFor="startDate">Início</Label>
          <Input
            id="startDate"
            type="datetime-local"
            aria-invalid={errors.startDate !== undefined}
            aria-describedby={errors.startDate ? 'startDate-error' : undefined}
            {...register('startDate')}
          />
          {errors.startDate && (
            <p id="startDate-error" role="alert" className="text-sm text-danger">
              {errors.startDate.message}
            </p>
          )}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="endDate">Fim</Label>
          <Input
            id="endDate"
            type="datetime-local"
            aria-invalid={errors.endDate !== undefined}
            aria-describedby={errors.endDate ? 'endDate-error' : undefined}
            {...register('endDate')}
          />
          {errors.endDate && (
            <p id="endDate-error" role="alert" className="text-sm text-danger">
              {errors.endDate.message}
            </p>
          )}
        </div>
      </div>

      {errors.root && (
        <p role="alert" className="text-sm text-danger">
          {errors.root.message}
        </p>
      )}

      <div className="flex flex-wrap gap-3">
        <Button type="submit" disabled={isSubmitting} aria-busy={isSubmitting}>
          {isSubmitting ? 'Reservando…' : 'Reservar'}
        </Button>
        <Button type="button" variant="ghost" onClick={onCancel}>
          Cancelar
        </Button>
      </div>
    </form>
  )
}
