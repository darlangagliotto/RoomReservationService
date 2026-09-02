/*
  Instancia unica do cliente, ligada ao token-store.

  O tratamento de 401 e registrado pelo AuthProvider em vez de importado daqui:
  o cliente nao conhece React nem rotas, e a reacao a sessao expirada e derrubar
  o estado de sessao — a saida para o login e consequencia do RequireAuth ver
  `session === null`, nao de navegacao imperativa.
*/

import { createApiClient } from '@/api/client'
import { getToken } from '@/auth/token-store'

let unauthorizedHandler: () => void = () => {}

export function setUnauthorizedHandler(handler: () => void): void {
  unauthorizedHandler = handler
}

export const apiClient = createApiClient({
  getToken,
  onUnauthorized: () => {
    unauthorizedHandler()
  },
})
