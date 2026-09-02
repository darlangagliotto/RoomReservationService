import { useAuth } from '@/auth/useAuth'
import { Button } from '@/components/ui/button'

/*
  Placeholder do passo 3. O AppShell e o estado vazio definitivo entram no
  passo 4 da spec 001.
*/
export function HomePage() {
  const { session, signOut } = useAuth()

  return (
    <main className="mx-auto flex min-h-dvh max-w-3xl flex-col gap-4 p-6">
      <header className="flex items-center justify-between gap-4">
        <h1 className="text-xl font-semibold tracking-tight">Room Reservation</h1>
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted">{session?.email}</span>
          <Button variant="ghost" size="sm" onClick={signOut}>
            Sair
          </Button>
        </div>
      </header>

      <p className="text-muted">Salas e reservas aparecerao aqui.</p>
    </main>
  )
}
