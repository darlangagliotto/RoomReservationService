import type { RouteObject } from 'react-router'
import { RequireAuth } from '@/auth/RequireAuth'
import { AppShell } from '@/components/AppShell'
import { LoginPage } from '@/features/auth/LoginPage'
import { EquipmentsPage } from '@/features/equipments/EquipmentsPage'
import { HomePage } from '@/features/home/HomePage'
import { ReservationsPage } from '@/features/reservations/ReservationsPage'
import { RoomsPage } from '@/features/rooms/RoomsPage'

/** Exportado separado do router para os testes montarem um memory router. */
export const routes: RouteObject[] = [
  { path: '/login', element: <LoginPage /> },
  {
    element: <RequireAuth />,
    children: [
      {
        element: <AppShell />,
        children: [
          { path: '/', element: <HomePage /> },
          { path: '/salas', element: <RoomsPage /> },
          { path: '/equipamentos', element: <EquipmentsPage /> },
          { path: '/reservas', element: <ReservationsPage /> },
        ],
      },
    ],
  },
]
