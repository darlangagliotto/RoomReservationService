import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createBrowserRouter, RouterProvider } from 'react-router'
import { AuthProvider } from '@/auth/AuthProvider'
import { routes } from '@/routes'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      // 401 e sessao expirada: repetir nao ajuda e atrasa a saida para o login.
      retry: false,
      refetchOnWindowFocus: false,
    },
  },
})

const router = createBrowserRouter(routes)

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <RouterProvider router={router} />
      </AuthProvider>
    </QueryClientProvider>
  )
}
