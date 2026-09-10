import { describe, expect, it } from 'vitest'
import { defaultReservationWindow, formatLocalDateTime, fromApiUtc, toApiUtc } from '@/lib/datetime'

describe('toApiUtc / fromApiUtc', () => {
  it('faz o caminho de ida e volta preservando o instante', () => {
    const local = '2026-09-10T14:00'
    const utc = toApiUtc(local)

    expect(fromApiUtc(utc)).toBe(local)
  })
})

describe('defaultReservationWindow', () => {
  it('arredonda para a próxima meia hora cheia quando faltam menos de 30 minutos', () => {
    const now = new Date(2026, 8, 10, 14, 10)

    const { start, end } = defaultReservationWindow(now)

    expect(start).toBe('2026-09-10T14:30')
    expect(end).toBe('2026-09-10T15:30')
  })

  it('vira a hora quando já passou da meia hora', () => {
    const now = new Date(2026, 8, 10, 14, 45)

    const { start, end } = defaultReservationWindow(now)

    expect(start).toBe('2026-09-10T15:00')
    expect(end).toBe('2026-09-10T16:00')
  })

  it('vira o dia quando o arredondamento cai à meia-noite', () => {
    const now = new Date(2026, 8, 10, 23, 45)

    const { start, end } = defaultReservationWindow(now)

    expect(start).toBe('2026-09-11T00:00')
    expect(end).toBe('2026-09-11T01:00')
  })
})

describe('formatLocalDateTime', () => {
  it('formata em pt-BR', () => {
    // Instante fixo em UTC; o teste roda com fuso do CI, entao so garantimos
    // o formato — nao um horario absoluto.
    const formatted = formatLocalDateTime('2026-09-10T14:00:00Z')

    expect(formatted).toMatch(/\d{2}\/\d{2}\/\d{4}/)
  })
})
