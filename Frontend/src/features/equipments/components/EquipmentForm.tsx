import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { EQUIPMENT_TYPE_LABELS, EQUIPMENT_TYPES, type EquipmentType } from '@/api/equipments'
import { ApiError } from '@/api/errors'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select } from '@/components/ui/select'
import { isFutureDate } from '@/lib/dates'

function isEquipmentType(value: string): value is EquipmentType {
  return (EQUIPMENT_TYPES as readonly string[]).includes(value)
}

/*
  Espelha as invariantes de docs/domain/model.md#equipment--raiz-roomservice.
  Espelhar nao e substituir: o servidor continua sendo a autoridade, e a
  mensagem dele e exibida como veio (ex.: numero de serie duplicado).
*/
const equipmentSchema = z.object({
  type: z.string().refine(isEquipmentType, 'Selecione um tipo de equipamento.'),
  brand: z.string().trim().min(3, 'A marca precisa de ao menos 3 caracteres.'),
  serialNumber: z.string().trim().min(3, 'O número de série precisa de ao menos 3 caracteres.'),
  purchaseDate: z
    .string()
    .min(1, 'Informe a data de compra.')
    .refine((v) => !isFutureDate(v), 'A data de compra não pode ser no futuro.'),
})

export type EquipmentFormValues = z.input<typeof equipmentSchema>
type ParsedEquipment = z.output<typeof equipmentSchema>

interface EquipmentFormProps {
  submitLabel: string
  onCancel: () => void
  onSubmit: (values: ParsedEquipment) => Promise<void>
}

export function EquipmentForm({ submitLabel, onCancel, onSubmit }: EquipmentFormProps) {
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm({
    resolver: zodResolver(equipmentSchema),
    defaultValues: { type: '', brand: '', serialNumber: '', purchaseDate: '' },
  })

  async function submit(values: ParsedEquipment) {
    try {
      await onSubmit(values)
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
        <Label htmlFor="type">Tipo</Label>
        <Select
          id="type"
          autoFocus
          aria-invalid={errors.type !== undefined}
          aria-describedby={errors.type ? 'type-error' : undefined}
          {...register('type')}
        >
          <option value="">Selecione…</option>
          {EQUIPMENT_TYPES.map((type) => (
            <option key={type} value={type}>
              {EQUIPMENT_TYPE_LABELS[type]}
            </option>
          ))}
        </Select>
        {errors.type && (
          <p id="type-error" role="alert" className="text-sm text-danger">
            {errors.type.message}
          </p>
        )}
      </div>

      <div className="grid gap-5 sm:grid-cols-2">
        <div className="flex flex-col gap-2">
          <Label htmlFor="brand">Marca</Label>
          <Input
            id="brand"
            aria-invalid={errors.brand !== undefined}
            aria-describedby={errors.brand ? 'brand-error' : undefined}
            {...register('brand')}
          />
          {errors.brand && (
            <p id="brand-error" role="alert" className="text-sm text-danger">
              {errors.brand.message}
            </p>
          )}
        </div>

        <div className="flex flex-col gap-2">
          <Label htmlFor="serialNumber">Número de série</Label>
          <Input
            id="serialNumber"
            aria-invalid={errors.serialNumber !== undefined}
            aria-describedby={errors.serialNumber ? 'serialNumber-error' : undefined}
            {...register('serialNumber')}
          />
          {errors.serialNumber && (
            <p id="serialNumber-error" role="alert" className="text-sm text-danger">
              {errors.serialNumber.message}
            </p>
          )}
        </div>
      </div>

      <div className="flex flex-col gap-2">
        <Label htmlFor="purchaseDate">Data de compra</Label>
        <Input
          id="purchaseDate"
          type="date"
          className="sm:max-w-xs"
          aria-invalid={errors.purchaseDate !== undefined}
          aria-describedby={errors.purchaseDate ? 'purchaseDate-error' : undefined}
          {...register('purchaseDate')}
        />
        {errors.purchaseDate && (
          <p id="purchaseDate-error" role="alert" className="text-sm text-danger">
            {errors.purchaseDate.message}
          </p>
        )}
      </div>

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
