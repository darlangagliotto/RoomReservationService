import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { ApiError, ValidationError } from '@/api/errors'
import type { Equipment, Room } from '@/api/rooms'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

/*
  Espelha as invariantes de docs/domain/model.md. Espelhar nao e substituir:
  unicidade de nome, numero e planSlot so o servidor sabe, e a mensagem dele
  e exibida como veio.
*/
const roomSchema = z.object({
  name: z.string().trim().min(3, 'O nome precisa de ao menos 3 caracteres.'),
  number: z.coerce.number<number>().int().positive('O número da sala precisa ser maior que 0.'),
  planSlot: z
    .union([z.literal(''), z.coerce.number<number>().int().min(1).max(10)])
    .optional()
    .transform((v) => (v === '' || v === undefined ? null : v)),
})

export type RoomFormValues = z.input<typeof roomSchema>
type ParsedRoom = z.output<typeof roomSchema>

interface RoomFormProps {
  room?: Room
  equipments?: Equipment[]
  submitLabel: string
  onCancel: () => void
  onSubmit: (values: ParsedRoom & { equipmentIds: string[] }) => Promise<void>
}

export function RoomForm({ room, equipments, submitLabel, onCancel, onSubmit }: RoomFormProps) {
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(roomSchema),
    defaultValues: {
      name: room?.name ?? '',
      number: room?.number ?? ('' as unknown as number),
      planSlot: (room?.planSlot ?? '') as unknown as number,
    },
  })

  async function submit(values: ParsedRoom, event?: React.BaseSyntheticEvent) {
    const form = event?.target as HTMLFormElement | undefined
    const equipmentIds = form
      ? Array.from(form.querySelectorAll<HTMLInputElement>('input[name="equipmentIds"]:checked')).map(
          (input) => input.value,
        )
      : []

    try {
      await onSubmit({ ...values, equipmentIds })
    } catch (error) {
      if (error instanceof ValidationError) {
        for (const [field, messages] of Object.entries(error.fieldErrors)) {
          const key = field.toLowerCase()
          if ((key === 'name' || key === 'number' || key === 'planslot') && messages[0]) {
            setError(key === 'planslot' ? 'planSlot' : (key as 'name' | 'number'), {
              message: messages[0],
            })
          }
        }
      } else if (error instanceof ApiError) {
        setError('root', { message: error.message })
      } else {
        throw error
      }
    }
  }

  return (
    <form onSubmit={handleSubmit(submit)} noValidate className="flex flex-col gap-5">
      <div className="flex flex-col gap-2">
        <Label htmlFor="name">Nome</Label>
        <Input
          id="name"
          autoFocus
          aria-invalid={errors.name !== undefined}
          aria-describedby={errors.name ? 'name-error' : undefined}
          {...register('name')}
        />
        {errors.name && (
          <p id="name-error" role="alert" className="text-sm text-danger">
            {errors.name.message}
          </p>
        )}
      </div>

      <div className="grid gap-5 sm:grid-cols-2">
        <div className="flex flex-col gap-2">
          <Label htmlFor="number">Número</Label>
          <Input
            id="number"
            type="number"
            inputMode="numeric"
            aria-invalid={errors.number !== undefined}
            aria-describedby={errors.number ? 'number-error' : undefined}
            {...register('number')}
          />
          {errors.number && (
            <p id="number-error" role="alert" className="text-sm text-danger">
              {errors.number.message}
            </p>
          )}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="planSlot">Posição na planta</Label>
          <Input
            id="planSlot"
            type="number"
            inputMode="numeric"
            placeholder="1 a 10 (opcional)"
            aria-invalid={errors.planSlot !== undefined}
            aria-describedby={errors.planSlot ? 'planSlot-error' : 'planSlot-help'}
            {...register('planSlot')}
          />
          {errors.planSlot ? (
            <p id="planSlot-error" role="alert" className="text-sm text-danger">
              {errors.planSlot.message}
            </p>
          ) : (
            <p id="planSlot-help" className="text-sm text-muted">
              Sem posição, a sala não aparece na planta.
            </p>
          )}
        </div>
      </div>

      {equipments && (
        <fieldset className="flex flex-col gap-2">
          <legend className="mb-2 text-sm font-medium">Equipamentos disponíveis</legend>
          {equipments.length === 0 ? (
            <p className="text-sm text-muted">
              Nenhum equipamento livre. Equipamento já alocado a outra sala não pode ser reutilizado.
            </p>
          ) : (
            <div className="flex flex-col gap-2">
              {equipments.map((equipment) => (
                <label key={equipment.id} className="flex items-center gap-3 text-sm">
                  <input
                    type="checkbox"
                    name="equipmentIds"
                    value={equipment.id}
                    className="size-4 accent-[var(--color-accent)]"
                  />
                  <span>
                    {equipment.type} · {equipment.brand}{' '}
                    <span className="text-muted">({equipment.serialNumber})</span>
                  </span>
                </label>
              ))}
            </div>
          )}
        </fieldset>
      )}

      {errors.root && (
        <p role="alert" className="text-sm text-danger">
          {errors.root.message}
        </p>
      )}

      <div className="flex flex-wrap gap-3">
        <Button type="submit" disabled={isSubmitting} aria-busy={isSubmitting}>
          {isSubmitting ? 'Salvando…' : submitLabel}
        </Button>
        <Button type="button" variant="ghost" onClick={onCancel}>
          Cancelar
        </Button>
      </div>
    </form>
  )
}
