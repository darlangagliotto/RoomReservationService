import { describe, expect, it } from 'vitest'
import { pickRoomImage } from '@/lib/roomImages'

describe('pickRoomImage', () => {
  it('é determinístico: o mesmo id sempre volta a mesma imagem', () => {
    const id = 'r-1234'

    expect(pickRoomImage(id)).toBe(pickRoomImage(id))
  })

  it('sempre devolve uma imagem do pool de 3', () => {
    const pool = ['/rooms/sala-1.jpg', '/rooms/sala-2.jpg', '/rooms/sala-3.jpg']

    for (const id of ['a', 'b', 'c', 'sala-azul', 'x-y-z-123']) {
      expect(pool).toContain(pickRoomImage(id))
    }
  })

  it('distribui ids diferentes por mais de uma imagem do pool', () => {
    const results = new Set(['r-1', 'r-2', 'r-3', 'r-4', 'r-5'].map(pickRoomImage))

    expect(results.size).toBeGreaterThan(1)
  })
})
