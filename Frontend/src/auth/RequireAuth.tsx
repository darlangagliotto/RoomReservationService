import { Navigate, Outlet, useLocation } from 'react-router'
import { useAuth } from '@/auth/useAuth'

/**
 * Guarda das rotas autenticadas. Sem sessao, envia ao login guardando a rota
 * pretendida em `state.from` para retorno apos autenticar.
 *
 * Tambem e o que reage a sessao expirada: quando um 401 zera a sessao, este
 * componente re-renderiza e redireciona — sem navegacao imperativa espalhada.
 */
export function RequireAuth() {
  const { session } = useAuth()
  const location = useLocation()

  if (session === null) {
    return <Navigate to="/login" replace state={{ from: location.pathname + location.search }} />
  }

  return <Outlet />
}
