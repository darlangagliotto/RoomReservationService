import { describe, expect, it } from 'vitest'
import { describeStatus } from '@/features/home/roomStatus'

describe('describeStatus', () => {
  it('mapeia Disponivel para Livre', () => {
    expect(describeStatus('Disponivel').label).toBe('Livre')
  })

  it('mapeia Reservada para Reservada', () => {
    expect(describeStatus('Reservada').label).toBe('Reservada')
  })

  it('mapeia EmUso para Em uso', () => {
    expect(describeStatus('EmUso').label).toBe('Em uso')
  })
})
