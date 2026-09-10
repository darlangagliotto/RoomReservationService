import type { RoomStatus } from '@/api/reservations'

/**
 * Mapa de status para rótulo e token de cor (spec 009, §5) — sempre bolinha
 * mais texto, nunca só cor. Um lugar só, usado pelo card e pelo painel de
 * detalhe para não divergir a nomenclatura entre os dois.
 */
export function describeStatus(status: RoomStatus): { label: string; colorVar: string } {
  switch (status) {
    case 'Disponivel':
      return { label: 'Livre', colorVar: 'var(--color-success)' }
    case 'Reservada':
      return { label: 'Reservada', colorVar: 'var(--color-accent)' }
    case 'EmUso':
      return { label: 'Em uso', colorVar: 'var(--color-muted)' }
  }
}
