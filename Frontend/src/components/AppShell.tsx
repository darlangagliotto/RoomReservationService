import { Outlet } from 'react-router'
import { useAuth } from '@/auth/useAuth'
import { Button } from '@/components/ui/button'

/**
 * Moldura das rotas autenticadas: identidade do produto e a sessao corrente.
 *
 * Nao ha barra de navegacao ainda — enquanto so existe a Home, um menu (ou um
 * hamburguer no mobile) seria decoracao apontando para lugar nenhum. O slot
 * entra quando as telas de salas e reservas existirem (spec 002).
 *
 * O shell exibe o e-mail porque o JWT nao carrega nome
 * (ver docs/services/auth-service.md).
 */
export function AppShell() {
  const { session, signOut } = useAuth()

  return (
    <div className="flex min-h-dvh flex-col">
      <header className="border-b border-border bg-surface">
        <div className="mx-auto flex max-w-5xl flex-wrap items-center justify-between gap-x-4 gap-y-2 px-4 py-3 sm:px-6">
          <span className="font-display text-base font-bold tracking-tight sm:text-lg">
            Room Reservation
          </span>

          <div className="flex items-center gap-2 sm:gap-3">
            <span className="max-w-[9rem] truncate text-sm text-muted sm:max-w-none">
              {session?.email}
            </span>
            <Button variant="ghost" size="sm" onClick={signOut}>
              Sair
            </Button>
          </div>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl flex-1 px-4 py-8 sm:px-6">
        <Outlet />
      </main>
    </div>
  )
}
