import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { Navigate, useLocation, useNavigate } from 'react-router'
import { z } from 'zod'
import { ApiError, ValidationError } from '@/api/errors'
import { useAuth } from '@/auth/useAuth'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'

/*
  O cliente valida apenas preenchimento e formato de e-mail.

  Comprimento minimo de senha NAO e validado aqui, apesar de o cadastro exigir
  6 caracteres: a regra pertence ao registro, e verifica-la no login revelaria a
  politica de senha a quem nao esta autenticado, alem de recusar localmente uma
  credencial que o servidor talvez aceite.
  Ver docs/specs/001-login-e-sessao.md#5-regras-de-negocio
*/
const loginSchema = z.object({
  email: z.email('Informe um e-mail válido.'),
  password: z.string().min(1, 'Informe sua senha.'),
})

type LoginValues = z.infer<typeof loginSchema>

export function LoginPage() {
  const { session, signIn } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const {
    register,
    handleSubmit,
    setError,
    resetField,
    setFocus,
    formState: { errors, isSubmitting },
  } = useForm<LoginValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  })

  if (session !== null) {
    return <Navigate to={from} replace />
  }

  async function onSubmit(values: LoginValues) {
    try {
      await signIn(values)
      await navigate(from, { replace: true })
    } catch (error) {
      if (error instanceof ValidationError) {
        for (const [field, messages] of Object.entries(error.fieldErrors)) {
          const key = field.toLowerCase()
          if ((key === 'email' || key === 'password') && messages[0]) {
            setError(key, { message: messages[0] })
          }
        }
      } else if (error instanceof ApiError) {
        setError('root', { message: error.message })
      } else {
        throw error
      }

      resetField('password')
      setFocus('password')
    }
  }

  return (
    <main className="blueprint-ground flex min-h-dvh items-center justify-center p-4">
      {/* A placa: o objeto em que se entra, sobre a malha da planta. */}
      <div className="w-full max-w-sm rounded-lg border border-border bg-surface p-6 shadow-sm sm:p-8">
        <h1 className="font-display text-2xl font-bold tracking-tight">Room Reservation</h1>
        <p className="mt-1 text-sm text-muted">Entre para reservar uma sala.</p>

        <form onSubmit={handleSubmit(onSubmit)} noValidate className="mt-8 flex flex-col gap-5">
          <div className="flex flex-col gap-2">
            <Label htmlFor="email">E-mail</Label>
            <Input
              id="email"
              type="email"
              autoComplete="username"
              autoFocus
              aria-invalid={errors.email !== undefined}
              aria-describedby={errors.email ? 'email-error' : undefined}
              {...register('email')}
            />
            {errors.email && (
              <p id="email-error" role="alert" className="text-sm text-danger">
                {errors.email.message}
              </p>
            )}
          </div>

          <div className="flex flex-col gap-2">
            <Label htmlFor="password">Senha</Label>
            <Input
              id="password"
              type="password"
              autoComplete="current-password"
              aria-invalid={errors.password !== undefined}
              aria-describedby={errors.password ? 'password-error' : undefined}
              {...register('password')}
            />
            {errors.password && (
              <p id="password-error" role="alert" className="text-sm text-danger">
                {errors.password.message}
              </p>
            )}
          </div>

          {errors.root && (
            <p role="alert" className="text-sm text-danger">
              {errors.root.message}
            </p>
          )}

          <Button type="submit" disabled={isSubmitting} aria-busy={isSubmitting}>
            {isSubmitting ? 'Entrando…' : 'Entrar'}
          </Button>
        </form>
      </div>
    </main>
  )
}
