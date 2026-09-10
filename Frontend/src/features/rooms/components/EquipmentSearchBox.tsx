import { useId, useMemo, useState, type KeyboardEvent } from 'react'
import type { Equipment } from '@/api/rooms'
import { Input } from '@/components/ui/input'
import { cn } from '@/lib/utils'

interface EquipmentSearchBoxProps {
  id: string
  /** Candidatos já filtrados pelo chamador (ex.: livres, ou livres e não selecionados ainda). */
  equipments: Equipment[]
  onSelect: (equipment: Equipment) => void
  /** Mostrada no lugar da busca quando não há candidato nenhum. */
  emptyMessage: string
  placeholder?: string
}

function matches(equipment: Equipment, query: string): boolean {
  const q = query.trim().toLowerCase()
  return (
    equipment.type.toLowerCase().includes(q) ||
    equipment.brand.toLowerCase().includes(q) ||
    equipment.serialNumber.toLowerCase().includes(q)
  )
}

/**
 * Busca por digitação em vez de lista de checkbox: com o patrimonio crescendo,
 * marcar um a um numa lista fixa vira o gargalo para achar o equipamento
 * certo. Combobox ARIA 1.2 (aria-activedescendant) — sugestões só aparecem
 * enquanto se digita, e tudo funciona só com teclado.
 */
export function EquipmentSearchBox({
  id,
  equipments,
  onSelect,
  emptyMessage,
  placeholder = 'Buscar por tipo, marca ou número de série…',
}: EquipmentSearchBoxProps) {
  const [query, setQuery] = useState('')
  const [highlighted, setHighlighted] = useState(0)
  const listboxId = useId()
  const optionId = (index: number) => `${listboxId}-${index}`

  const options = useMemo(
    () => (query.trim() === '' ? [] : equipments.filter((e) => matches(e, query))),
    [equipments, query],
  )
  const isOpen = options.length > 0

  function select(equipment: Equipment) {
    onSelect(equipment)
    setQuery('')
    setHighlighted(0)
  }

  function onKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (!isOpen) return

    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setHighlighted((i) => (i + 1) % options.length)
    } else if (event.key === 'ArrowUp') {
      event.preventDefault()
      setHighlighted((i) => (i - 1 + options.length) % options.length)
    } else if (event.key === 'Enter') {
      event.preventDefault()
      const option = options[highlighted]
      if (option) select(option)
    } else if (event.key === 'Escape') {
      setQuery('')
    }
  }

  if (equipments.length === 0) {
    return <p className="text-sm text-muted">{emptyMessage}</p>
  }

  return (
    <div className="relative">
      <Input
        id={id}
        role="combobox"
        aria-expanded={isOpen}
        aria-controls={listboxId}
        aria-autocomplete="list"
        aria-activedescendant={isOpen ? optionId(highlighted) : undefined}
        autoComplete="off"
        placeholder={placeholder}
        value={query}
        onChange={(event) => {
          setQuery(event.target.value)
          setHighlighted(0)
        }}
        onKeyDown={onKeyDown}
      />

      {query.trim() !== '' &&
        (isOpen ? (
          <ul
            id={listboxId}
            role="listbox"
            aria-label="Equipamentos encontrados"
            className="absolute z-10 mt-1 max-h-56 w-full overflow-y-auto rounded-md border border-border bg-surface shadow-lg"
          >
            {options.map((equipment, index) => (
              <li
                key={equipment.id}
                id={optionId(index)}
                role="option"
                aria-selected={index === highlighted}
                onMouseEnter={() => setHighlighted(index)}
                onClick={() => select(equipment)}
                className={cn(
                  'cursor-pointer px-3 py-2 text-sm',
                  index === highlighted ? 'bg-accent/15' : 'hover:bg-border/50',
                )}
              >
                {equipment.type} · {equipment.brand}{' '}
                <span className="text-muted">({equipment.serialNumber})</span>
              </li>
            ))}
          </ul>
        ) : (
          <p className="mt-1 text-sm text-muted">Nenhum equipamento encontrado.</p>
        ))}
    </div>
  )
}
