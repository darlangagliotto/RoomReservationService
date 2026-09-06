---
name: frontend-app
description: Constrói e evolui o frontend React + TypeScript do RoomReservationService. Use ao criar tela, página, componente, formulário ou fluxo de UI; ao integrar o frontend com o Gateway/API; ao lidar com login, token JWT, rotas protegidas, estados de erro/vazio/carregamento; ou ao configurar o app em Frontend/. Cobre stack, arquitetura, integração, segurança e responsividade — a identidade visual vem da skill frontend-design.
---

# Frontend do RoomReservationService

App React + TypeScript em `Frontend/`, consumindo a API pelo Gateway. Este documento
define **como se constrói**; a personalidade visual (paleta, tipografia, layout) vem da
skill `frontend-design`. As duas não se sobrepõem: aqui ficam tokens como contrato,
lá ficam os valores concretos.

Contexto do backend: [docs/architecture/overview.md](../../../docs/architecture/overview.md).
Contratos por serviço: [docs/services/](../../../docs/services/).

## Antes de qualquer tela: um bloqueador de backend

**Login e cadastro seguem bloqueados no Gateway** ([B2](../../../docs/sdd/backlog.md#b2)):
a `FallbackPolicy` exige token em `/api/auth/*` e `POST /api/users` — justamente as rotas
de quem ainda não tem token. O desvio está em **dois** lugares, ambos com `TODO(B2)`:
`vite.config.ts` (dev) e `Frontend/nginx.conf` (container). Ao corrigir B2, remova os dois.

**CORS deixou de ser problema**: o nginx serve o bundle e faz o proxy de `/api`, então
aplicação e API ficam na mesma origem. Só volta a importar se o frontend for servido de
outro host.


## Stack fixa

Fixada em **2026-09-02**. Não troque item sem registrar aqui e justificar; não persiga
versões novas por serem novas. Antes de fazer o scaffold, confirme o major atual de
cada pacote (`npm view <pacote> version`) e, se divergir, **atualize esta lista** em vez
de improvisar.

| Papel | Escolha |
|---|---|
| Build / dev server | Vite |
| UI | React + TypeScript (`strict: true`, sem `any`) |
| Roteamento | React Router (data router) |
| Dados de servidor | TanStack Query |
| Estilo | Tailwind CSS (config CSS-first via `@theme`) |
| Componentes | shadcn/ui (copiados para `src/components/ui`, versionados no repo) |
| Formulários | react-hook-form + zod |
| Datas | date-fns (com utilitários UTC próprios, ver abaixo) |
| Testes | Vitest + Testing Library |

Versões efetivamente instaladas em 2026-09-02 (passo 1 da spec 001): React `19.2`,
Vite `8.2`, TypeScript `7.0`, Tailwind `4.3`, `@vitejs/plugin-react` `6.1`.

**Armadilha do TypeScript 7**: a opção `baseUrl` foi **removida**. Alias de path se
declara só com `paths`, e o destino precisa ser relativo — `"@/*": ["./src/*"]`, com o
`./` obrigatório. Configuração herdada de projetos TS 5 falha no `tsc` com `TS5102`/`TS5090`.

Nada de biblioteca de estado global (Redux, Zustand) enquanto o estado for
majoritariamente de servidor — TanStack Query já é o cache. Estado de UI fica em
`useState`/`useReducer` local ou Context pequeno.

## Estrutura

```
Frontend/
├── Dockerfile                  build multi-stage → nginx, servindo na 5005
├── vite.config.ts              proxy /api para o Gateway
├── src/
│   ├── main.tsx  App.tsx  router.tsx
│   ├── api/
│   │   ├── client.ts           fetch wrapper: baseUrl, Authorization, parse de erro
│   │   ├── errors.ts           ApiError, BusinessError, ValidationError
│   │   └── <recurso>.ts        auth.ts, users.ts, rooms.ts, reservations.ts
│   ├── auth/
│   │   ├── token-store.ts      memória + sessionStorage
│   │   ├── AuthProvider.tsx    contexto de sessão
│   │   └── RequireAuth.tsx     guarda de rota
│   ├── features/<dominio>/     rooms/, reservations/, users/
│   │   ├── components/  hooks/  <Tela>Page.tsx
│   ├── components/
│   │   ├── ui/                 shadcn (não editar à mão sem motivo)
│   │   └── <compartilhados>/   AppShell, EmptyState, ErrorState, PageHeader
│   ├── lib/                    utils.ts, datetime.ts
│   └── styles/globals.css      @theme com os tokens
└── tests/
```

Uma feature = uma pasta em `features/`, espelhando o serviço backend correspondente.
Componente só sobe para `components/` quando for usado por duas features.

Registre o serviço em `Infra/docker-compose.yml` (porta host 5005) e a pasta no
`.gitignore` para `node_modules/` e `dist/`.

## Integração com a API

### Base URL e proxy

Em dev, **não chame o backend por origem cruzada** — use o proxy do Vite, que elimina
CORS e mantém o código igual em dev e produção:

```ts
// vite.config.ts
server: {
  proxy: {
    // TODO(B2): voltar para o Gateway quando /api/auth for liberado no Gateway
    '/api/auth':  { target: 'http://localhost:5001', changeOrigin: true },
    '/api/users': { target: 'http://localhost:5002', changeOrigin: true },
    '/api':       { target: 'http://localhost:5000', changeOrigin: true },
  },
}
```

O código da aplicação chama sempre caminhos relativos (`/api/rooms`), nunca host fixo.

### Tratamento de resposta — o mapa que importa

O backend tem três formatos de erro e uma armadilha. O `client.ts` deve normalizar
tudo isso **em um lugar só**, e nenhuma tela deve inspecionar status HTTP cru.

| Resposta do backend | Significado real | O que a UI faz |
|---|---|---|
| `2xx` | sucesso | renderiza |
| `400` + `ProblemDetails` com `title: "Erro de negócio"` | regra de negócio violada | mostra `detail` como mensagem, próxima da ação |
| `400` + `ValidationProblemDetails` (tem `errors`) | validação de entrada | mostra erro por campo no formulário |
| `400` com `detail` como `"Nenhuma sala encontrada."` | **lista vazia, não erro** | renderiza estado vazio |
| `401` | token ausente, inválido ou expirado | encerra sessão e vai para login |
| `500` + `ProblemDetails` | falha inesperada | estado de erro genérico + opção de repetir |

A quarta linha é a mais fácil de errar: **o backend devolve `400` para resultado vazio**
(consequência do ADR-011). Trate como coleção vazia no `client.ts`, convertendo em
`[]`, para que nenhuma tela mostre "erro" numa busca legítima sem resultados.

### Datas

Colunas são `timestamptz` e o Npgsql exige UTC — enviar data com offset local causa
`500`. Centralize em `lib/datetime.ts`: `toApiUtc(date)` na saída e `fromApiUtc(iso)` na
entrada. Nenhum componente monta string de data à mão.

### Dados que o backend ainda não fornece

- O JWT carrega apenas `sub`, `email` e `jti` — **sem nome e sem papel**. O app shell
  exibe o e-mail. `GET /api/users/{id}` existe desde a spec 002 e devolve o nome, mas
  custa uma chamada por usuário: use-o onde o nome importa, não no shell.
- Sem claim de papel, **não há autorização por perfil**: não construa UI condicional a
  "admin". A autorização é binária (autenticado ou não).

## Autenticação e segurança

**Armazenamento do token**: memória (fonte de verdade) + cópia em `sessionStorage`
apenas para sobreviver ao F5 da aba. Nunca `localStorage`. Implementado em
`auth/token-store.ts`; nenhum outro arquivo lê o storage diretamente.

**Expiração**: o token vale 60 minutos e **não há refresh token**. Não tente renovar.
O `client.ts` intercepta `401` → limpa o store → redireciona para `/login` preservando a
rota de origem. Um aviso antes de expirar é opcional; renovação silenciosa não é possível.

**Logout** = limpar memória e `sessionStorage`. Não existe revogação no servidor; não
prometa na UI que "a sessão foi encerrada em todos os dispositivos".

Regras não negociáveis:

- **Nenhum segredo no frontend.** Tudo em `import.meta.env.VITE_*` vai para o bundle e é
  público. `JWT_KEY`, connection strings e afins nunca aparecem aqui — nem em `.env`,
  nem em comentário, nem em exemplo de código.
- **Nunca logar token, senha ou corpo de request de login** (`console.log` incluso).
- Senha só em `<input type="password">` com `autoComplete` correto; nunca em query
  string, nunca em estado que persista.
- Toda renderização de texto vindo da API passa pelo escape padrão do React.
  `dangerouslySetInnerHTML` exige justificativa escrita no código.
- Dependência nova entra por decisão consciente: verifique manutenção e peso antes.

## Estado e dados

TanStack Query para tudo que vem do servidor. Convenções:

- Query keys hierárquicas: `['rooms']`, `['rooms', { name, number }]`, `['reservations', filtros]`.
- Um hook por operação em `features/<dominio>/hooks/`: `useRooms`, `useCreateReservation`.
- Mutação invalida as queries afetadas; sem atualização otimista onde o backend valida
  regra que o frontend não replica (overlap de reserva é decidido no servidor).
- Componente não chama `fetch` direto — sempre pelo hook.

## Responsividade

**Mobile-first**: escreva o layout base para 375px e adicione breakpoints para cima.
Nunca o contrário.

| Faixa | Alvo |
|---|---|
| 375–639px | coluna única; navegação recolhida; tabela vira lista de cards |
| 640–1023px | duas colunas onde couber; navegação em drawer |
| ≥1024px | layout completo com navegação lateral persistente |

Regras verificáveis:

- Nenhum scroll horizontal em 375px. Conteúdo largo (tabela, planta baixa) rola dentro
  do próprio container com `overflow-x: auto`.
- Alvo de toque mínimo de 44×44px em controles primários.
- Tipografia e espaçamento vêm da escala de tokens; nada de `px` mágico solto.
- Teste em 375, 768 e 1440 antes de considerar a tela pronta.

## Formulários

react-hook-form + zod. O schema zod **espelha as invariantes do domínio** documentadas em
[docs/domain/model.md](../../../docs/domain/model.md) — nome ≥ 3 caracteres, senha ≥ 6,
número da sala > 0, início da reserva no futuro, fim depois do início.

Espelhar não é substituir: **a validação do servidor é a autoridade**. Erro `400` de
negócio que chega mesmo com o formulário válido deve ser exibido, não engolido.

Todo campo tem `<label>` associado; erro é anunciado com `aria-describedby` e
`aria-invalid`; o foco vai para o primeiro campo inválido no submit.

## Acessibilidade — piso obrigatório

- HTML semântico antes de ARIA. `<button>` para ação, `<a>` para navegação.
- Foco visível em tudo que é focável; ordem de tabulação segue a ordem visual.
- Contraste mínimo AA (4.5:1 em texto normal) — vale também para os tokens que a
  `frontend-design` propuser; contraste vence estética.
- `prefers-reduced-motion` respeitado em toda animação.
- Diálogo/drawer prende o foco e fecha com `Esc`.
- Nenhuma informação transmitida só por cor (status de sala precisa de rótulo ou ícone).

## Estados de tela

Toda tela que carrega dados trata **quatro** estados explicitamente. Ausência de
qualquer um é bug, não detalhe:

1. **Carregando** — skeleton com a forma do conteúdo final, não spinner centralizado.
2. **Vazio** — `EmptyState` com uma frase do que aconteceu e a ação seguinte (lembre:
   chega como `400`).
3. **Erro** — mensagem em linguagem de produto + botão de repetir. Nunca exibir stack
   trace nem o `ProblemDetails` cru.
4. **Sucesso** — o conteúdo.

## Testabilidade

Todo módulo nasce testável — decisão separada de I/O:

- `fetch` existe **apenas** em `api/client.ts`. Componente e hook nunca chamam rede direto.
- `sessionStorage` é tocado **apenas** por `auth/token-store.ts`.
- Lógica (parse de erro, conversão UTC, resolução de filtro, decodificação do token)
  mora em função pura em `lib/` ou `api/`, exercitável sem React nem DOM.
- Componente recebe dado por prop ou hook; não busca por conta própria.

**Testável não é o mesmo que testado**: escreva teste onde a regra é não-óbvia — mapa de
erros, ciclo de sessão, conversão de data, resolução de filtro — não para renderização
trivial nem para repetir a implementação.

Ambiente: `vitest` com `environment: 'jsdom'` e `environmentOptions.jsdom.url` definido —
sem uma URL de origem explícita o jsdom não expõe storage. **No Node 26 o global
`localStorage` é nativo e exige `--localstorage-file`**, ficando indefinido e sombreando o
do jsdom; teste que precise dele deve injetar um dublê com `vi.stubGlobal`
(`unstubGlobals: true` no config). `sessionStorage` não é afetado. Como os helpers do
vitest são importados explicitamente (`globals: false`), o cleanup automático do Testing
Library **não** se registra sozinho: `afterEach(cleanup)` fica no `src/test/setup.ts` —
sem ele, renders anteriores permanecem no DOM e as queries encontram a tela errada.

## Design e tokens

A `frontend-design` decide paleta, tipografia e layout. Esta skill define o **contrato**
que aqueles valores devem cumprir:

- Tokens declarados em `src/styles/globals.css` no bloco `@theme` do Tailwind — cor,
  escala tipográfica, espaçamento, raio, sombra. Componente nunca usa hex literal.
- Cor por **papel semântico**, não por matiz: `--color-surface`, `--color-muted`,
  `--color-danger`, `--color-success`. Trocar a paleta não deve exigir tocar em
  componente.
- Tema claro e escuro definidos nos mesmos tokens.
- Componentes do shadcn são o piso; sobrescrever é permitido e esperado — o layout deve
  refletir o negócio (salas, equipamentos, reservas, ocupação), não parecer demo de
  biblioteca.

**Layout segue o negócio**: antes de desenhar uma tela, releia o caso de uso
correspondente em [docs/services/](../../../docs/services/). A tela de reserva é sobre
tempo e conflito; a de salas é sobre inventário e alocação. Um dashboard genérico com
quatro cards de métrica não descreve nenhum dos dois.

> A tela de planta baixa depende de conceitos que **não existem no domínio**: posição,
> andar, capacidade e "status agora" ([B16](../../../docs/sdd/backlog.md)). Modelagem no
> backend é pré-requisito — não improvise coordenadas no frontend.

## Checklist antes de concluir

- [ ] `npx tsc --noEmit` limpo; nenhum `any` novo
- [ ] Build (`npm run build`) sem erro nem warning novo
- [ ] Os quatro estados de tela implementados
- [ ] Erro de negócio, validação, `401` e `500` distinguidos e exibidos corretamente
- [ ] Lista vazia (`400`) renderiza estado vazio, não erro
- [ ] Sem scroll horizontal em 375px; conferido em 375 / 768 / 1440
- [ ] Navegação completa por teclado, com foco visível
- [ ] Contraste AA nos textos
- [ ] Datas convertidas para UTC na saída
- [ ] Nenhum segredo, token ou senha em log, `.env` versionado ou bundle
- [ ] Nova rota de API registrada em `src/api/` e no proxy do Vite, se necessário
- [ ] Lógica extraída de componente para função pura testável; teste onde a regra não é óbvia
- [ ] Documentação atualizada se algo aqui mudou (stack, contrato, estrutura)

## Idioma

| O quê | Idioma |
|---|---|
| Identificadores, tipos, nomes de arquivo, chaves de query | **inglês** |
| Texto que o usuário lê — rótulos, mensagens, estados vazios, erros | **português** |
| Comentários e descrições de teste | português |

A mensagem de erro de negócio **vem pronta do backend em português** e é exibida como
veio. O frontend não traduz nem reescreve: se o texto estiver ruim, o conserto é no
backend. Ver [conventions.md](../../../docs/architecture/conventions.md#idioma).
