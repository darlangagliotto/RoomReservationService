import { useCallback, useEffect, useMemo, useState, type ReactNode } from 'react'
import { login, type LoginRequest } from '@/api/auth'
import { apiClient, setUnauthorizedHandler } from '@/api'
import { AuthContext, type Session } from '@/auth/auth-context'
import { clearToken, restoreToken, setToken } from '@/auth/token-store'
import { getEmail } from '@/lib/jwt'

function toSession(token: string): Session {
  return { token, email: getEmail(token) }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  /*
    Restauracao e sincrona no primeiro render: evita o flash de tela de login
    para quem ja tem sessao valida. Token expirado e descartado pelo
    restoreToken, sem ida ao servidor.
  */
  const [session, setSession] = useState<Session | null>(() => {
    const token = restoreToken()
    return token === null ? null : toSession(token)
  })

  const signOut = useCallback(() => {
    clearToken()
    setSession(null)
  }, [])

  // Qualquer 401 derruba a sessao; o RequireAuth cuida do redirecionamento.
  useEffect(() => {
    setUnauthorizedHandler(signOut)
    return () => {
      setUnauthorizedHandler(() => {})
    }
  }, [signOut])

  const signIn = useCallback(async (credentials: LoginRequest) => {
    const { token } = await login(apiClient, credentials)
    setToken(token)
    setSession(toSession(token))
  }, [])

  const value = useMemo(() => ({ session, signIn, signOut }), [session, signIn, signOut])

  return <AuthContext value={value}>{children}</AuthContext>
}
