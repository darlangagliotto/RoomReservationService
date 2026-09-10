/**
 * Distribui um id de sala entre o pool fixo de ilustrações (spec 009). A
 * mesma sala sempre cai na mesma imagem — hash determinístico, não aleatório
 * a cada render, senão a imagem "piscaria" a cada re-fetch da lista.
 */
const ROOM_IMAGES = ['/rooms/sala-1.jpg', '/rooms/sala-2.jpg', '/rooms/sala-3.jpg'] as const

function hashString(value: string): number {
  let hash = 0
  for (let i = 0; i < value.length; i++) {
    hash = (hash * 31 + value.charCodeAt(i)) | 0
  }
  return Math.abs(hash)
}

export function pickRoomImage(roomId: string): string {
  const index = hashString(roomId) % ROOM_IMAGES.length
  return ROOM_IMAGES[index] ?? ROOM_IMAGES[0]
}
