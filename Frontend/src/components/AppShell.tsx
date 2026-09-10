import { NavLink, Outlet } from 'react-router'
import { useAuth } from '@/auth/useAuth'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

/*
  Moldura das rotas autenticadas.

  A barra so lista destinos que existem de verdade: a Planta entra quando a
  tela existir (spec 009, que depende de arte da planta baixa). Item de menu
  apontando para lugar nenhum e decoracao.

  Exibe o e-mail porque o JWT nao carrega nome (docs/services/auth-service.md).
*/
const sections = [
  { to: '/', label: 'Início', end: true },
  { to: '/salas', label: 'Salas', end: false },
  { to: '/equipamentos', label: 'Equipamentos', end: false },
  { to: '/reservas', label: 'Reservas', end: false },
]

export function AppShell() {
  const { session, signOut } = useAuth()

  return (
    <div className="flex min-h-dvh flex-col">
      <header className="border-b border-border bg-surface">
        <div className="mx-auto flex max-w-5xl flex-wrap items-center gap-x-4 gap-y-2 px-4 py-3 sm:px-6">
          <span className="font-display text-base font-bold tracking-tight sm:text-lg">
            Room Reservation
          </span>

          <nav aria-label="Seções" className="order-3 w-full sm:order-2 sm:w-auto">
            <ul className="flex items-center gap-1">
              {sections.map((section) => (
                <li key={section.to}>
                  <NavLink
                    to={section.to}
                    end={section.end}
                    className={({ isActive }) =>
                      cn(
                        'inline-flex h-9 items-center rounded-md px-3 text-sm font-medium transition-colors',
                        isActive
                          ? 'bg-accent/15 text-foreground'
                          : 'text-muted hover:bg-border/50 hover:text-foreground',
                      )
                    }
                  >
                    {section.label}
                  </NavLink>
                </li>
              ))}
            </ul>
          </nav>

          <div className="order-2 ml-auto flex items-center gap-2 sm:order-3 sm:gap-3">
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
