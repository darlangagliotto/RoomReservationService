import type { RouteObject } from 'react-router'
import { RequireAuth } from '@/auth/RequireAuth'
import { LoginPage } from '@/features/auth/LoginPage'
import { HomePage } from '@/features/home/HomePage'

/** Exportado separado do router para os testes montarem um memory router. */
export const routes: RouteObject[] = [
  { path: '/login', element: <LoginPage /> },
  {
    element: <RequireAuth />,
    children: [{ path: '/', element: <HomePage /> }],
  },
]
