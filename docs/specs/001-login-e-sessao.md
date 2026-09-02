# 001 — Login e sessão autenticada

| Campo | Valor |
|---|---|
| Status | Implementada |
| Serviços afetados | Frontend (novo), AuthService (consumo, sem alteração) |
| Depende de | — |
| Autor / data | Darlan · 2026-09-02 |

## 1. Problema

Não existe frontend. Hoje a única forma de exercitar a API é pelo Swagger de cada
serviço, colando o JWT manualmente a cada chamada. Sem uma sessão autenticada no
navegador, nenhuma tela de negócio (salas, reservas) pode ser construída — toda rota da
API, exceto login e cadastro, exige `Authorization: Bearer`.

Esta spec entrega a **primeira fatia vertical** do frontend: o caminho completo de
autenticação. Ela existe tanto para liberar as telas seguintes quanto para validar, na
prática, as decisões de arquitetura registradas na skill `frontend-app` antes que
qualquer outra tela dependa delas.

## 2. Resultado esperado

Uma pessoa abre o app, informa e-mail e senha, entra, e permanece autenticada enquanto
navega e ao recarregar a aba. Se tentar acessar uma rota protegida sem sessão, é levada
ao login e devolvida ao destino original depois de autenticar. Quando o token expira,
a sessão é encerrada de forma previsível, sem tela quebrada. O logout é explícito.

## 3. Escopo

**Incluído**

- Projeto React + TypeScript em `Frontend/`, com a stack fixada na skill `frontend-app`
- Proxy de desenvolvimento no Vite (contorna CORS e o bloqueio B2 do Gateway)
- `api/client.ts` com o mapa completo de erros do backend
- `auth/token-store.ts` (memória + `sessionStorage`), `AuthProvider`, `RequireAuth`
- Tela de login com validação, estados de carregamento e erro
- `AppShell` responsivo com o nome do produto, identificação do usuário e ação de logout
- **Home** (`/`), rota autenticada de destino após o login, com conteúdo placeholder
- Testes dos comportamentos de sessão

**Ampliações decididas durante a execução** (registradas aqui para o escopo não mentir
sobre o que foi entregue):

- Dockerfile do frontend, `nginx.conf` e serviço no `docker-compose` (porta 5005), a
  pedido, para a pilha inteira subir por Docker
- Tokens de cor em tema claro **e** escuro — a estrutura de tokens exige os dois papéis
  definidos juntos, e separá-los custaria mais do que fazê-los de uma vez
- Usuário fixo de desenvolvimento no UserService, necessário para verificar o fluxo real
  sem cadastrar conta à mão (ver [user-service.md](../services/user-service.md#seed-de-desenvolvimento))

**Fora de escopo**

- Cadastro de usuário (`POST /api/users`) — spec própria
- Qualquer tela de salas, equipamentos ou reservas
- Correção de B2 — contornada por desvio de proxy, não resolvida
- Refresh token, "lembrar-me", recuperação de senha

## 4. Contrato

Nenhum endpoint novo. Consome o contrato existente do
[AuthService](../services/auth-service.md).

### `POST /api/auth/login` — anônimo

Request:
```json
{ "email": "joao@email.com", "password": "123456" }
```

`200 OK`:
```json
{ "token": "<jwt>", "expiresAt": "2026-09-02T18:30:00Z" }
```

Erros:

| Condição | Resposta | `detail` |
|---|---|---|
| Credencial inválida, usuário bloqueado ou inexistente | `400` `ProblemDetails` | `"Invalid email or password!"` |
| UserService indisponível | `400` `ProblemDetails` | `"Invalid email or password!"` |
| Falha inesperada | `500` `ProblemDetails` | genérico |

O backend **não distingue** credencial errada de indisponibilidade do UserService — o
erro remoto é logado e convertido em credencial inválida. A UI herda essa limitação e
não deve inventar uma mensagem de indisponibilidade que não pode comprovar.

### Roteamento

O Gateway hoje bloqueia `/api/auth/*` pela `FallbackPolicy`
([B2](../sdd/backlog.md#b2)). O desvio é feito em dois lugares, ambos marcados com
`TODO(B2)`: `vite.config.ts`, no dev server, e `Frontend/nginx.conf`, no container. Os
dois enviam `/api/auth` e `/api/users` direto aos serviços e o restante de `/api` ao
Gateway.

O app sempre chama caminhos relativos (`/api/auth/login`); nenhum host aparece no código
da aplicação.

## 5. Regras de negócio

### Validação do formulário

| Campo | Regra no cliente |
|---|---|
| `email` | obrigatório; formato de e-mail |
| `password` | obrigatório |

**Exceção deliberada à convenção**: a skill `frontend-app` manda espelhar as invariantes
do domínio, e `RegisterUserRequestValidator` exige senha ≥ 6 caracteres. No **login**
essa regra não se aplica — validar comprimento aqui revela a política de senha a quem
não está autenticado e recusa localmente credenciais que o servidor talvez aceite. O
servidor é a autoridade sobre a senha; o cliente só verifica preenchimento.

### Sessão

- O token é a fonte de verdade da sessão; vale 60 minutos e **não há renovação**.
- `sessionStorage` guarda apenas o token, sob a chave `rrs.auth.token`. Nada mais é
  persistido. Não usar `localStorage`.
- Ao iniciar o app, o token guardado é lido e sua expiração (`exp`) verificada
  localmente: se já expirou, é descartado e a sessão não é restaurada — evita uma
  chamada inútil que retornaria `401`.
- Qualquer `401` de qualquer chamada encerra a sessão e leva ao login, preservando a
  rota de origem para retorno após autenticar.
- Logout limpa memória e `sessionStorage` e leva ao login. Não há revogação no servidor;
  a UI não deve afirmar que a sessão foi encerrada em outros dispositivos.

### Nomenclatura e destino

O produto chama-se **Room Reservation**. O nome aparece no `AppShell`, no `<title>` do
documento e na tela de login. `RoomReservationService` permanece como nome do
repositório e dos assemblies — não é usado na interface.

Após autenticar, o destino é a **Home** (`/`). Nesta spec a Home é um placeholder: um
estado vazio que nomeia o que virá ("Salas e reservas aparecerão aqui"), sem métricas,
gráficos ou cards inventados. O conteúdo real da Home será especificado quando as telas
de salas e reservas existirem e houver dado verdadeiro para mostrar.

### Identificação do usuário

O JWT carrega apenas `sub`, `email` e `jti` — **não há nome nem papel**, e
`GET /api/users/id/{id}` não existe ([B1](../sdd/backlog.md#b1)). O shell exibe o
**e-mail**, lido do claim `email` decodificado do próprio token.

A decodificação é **para exibição apenas**. Nenhuma decisão de autorização se apoia no
conteúdo lido no cliente — quem autoriza é o backend, a cada requisição. Não construir
UI condicional a perfil: a autorização do sistema é binária.

### Tratamento de resposta

O `client.ts` normaliza as quatro faixas descritas na skill `frontend-app` (negócio,
validação, `401`, `500`) e converte o `400` de "lista vazia" em coleção vazia. Nesta
spec só as faixas de negócio e `401` são exercitadas, mas o mapa completo é implementado
agora, porque as telas seguintes dependem dele.

## 6. Impacto em dados

Nenhuma alteração de banco, entidade ou migration.

Único dado persistido no cliente: `sessionStorage["rrs.auth.token"]`, apagado no logout,
na expiração e ao fechar a aba.

## 7. Impacto entre serviços

O frontend passa a chamar o AuthService, que já chama o UserService. Nenhum contrato de
backend muda; nenhuma ordem de implantação é imposta.

Indisponibilidade: com o AuthService fora do ar, o login falha com erro genérico; com o
UserService fora do ar, falha como credencial inválida (§4). Ambos os casos deixam a
pessoa na tela de login com mensagem — nunca em tela branca.

**CORS deixou de ser pendência.** Como o nginx serve o bundle e faz o proxy de `/api`,
aplicação e API ficam na mesma origem (`localhost:5005`) e o navegador nunca emite
requisição cross-origin. CORS no Gateway só volta a ser necessário se o frontend passar a
ser servido de outro host. O desbloqueio de `/api/auth/*` (B2) segue pendente — está
contornado por desvio de proxy, tanto no Vite quanto no nginx.

## 8. Critérios de aceite

> Verificados em 2026-09-02 com a pilha inteira em Docker (`localhost:5005`), contra
> Postgres, AuthService e UserService reais — não contra dublês. Os itens de sessão têm
> cobertura automatizada adicional em `src/auth/session.test.tsx`.

**Autenticação**

- [x] Dado credencial válida, quando submeto o login, então sou levado à Home (`/`) e o meu e-mail aparece no shell.
- [x] Dado credencial inválida, quando submeto, então vejo `"Invalid email or password!"` junto ao formulário, o campo de senha é limpo e permaneço no login.
- [x] Dado o AuthService indisponível, quando submeto, então vejo mensagem de erro e opção de tentar novamente, sem tela quebrada.
- [x] Dado e-mail em formato inválido, quando submeto, então vejo o erro no campo e nenhuma requisição é enviada.
- [x] Dado que digito uma senha de 3 caracteres, quando submeto, então a requisição **é** enviada (o cliente não valida comprimento).

**Sessão**

- [x] Dado que não tenho sessão, quando acesso `/` direto pela URL, então sou levado ao login e, após autenticar, chego na Home.
- [x] Dado que recarrego a aba autenticado, então permaneço autenticado.
- [x] Dado que fecho a aba e reabro, então **não** permaneço autenticado.
- [x] Dado um token expirado em `sessionStorage`, quando abro o app, então inicio deslogado e nenhuma requisição à API é feita.
- [x] Dado token expirado durante o uso, quando faço qualquer chamada, então a sessão é encerrada e vou para o login.
- [x] Dado que faço logout, então o token some da memória e do `sessionStorage`, e voltar pelo histórico não restaura a sessão.

**Interface**

- [x] Em 375px, login e shell não produzem scroll horizontal (medido: `scrollWidth === clientWidth` nas duas rotas).
- [—] ~~Navegação do shell recolhida em 375px~~ — **sem objeto**: não há itens de navegação enquanto só existe a Home, e um menu apontando para lugar nenhum seria decoração. O critério passa a valer na spec 002.
- [x] Todo o fluxo (login, navegação, logout) é percorrível apenas com teclado, com foco visível.
- [x] Durante o envio do login, o botão indica carregamento e não permite envio duplicado.
- [x] Nenhum token, senha ou corpo de requisição de login aparece no console.

**Qualidade**

- [x] `npx tsc --noEmit` limpo, sem `any`.
- [x] `npm run build` sem erro nem warning novo.
- [x] Testes cobrindo: restauração de sessão, expiração local, `401` durante o uso e logout.

## 9. Decisões em aberto

Nenhuma. As duas pendências foram resolvidas e incorporadas ao §5:

1. Nome do produto: **Room Reservation**.
2. Destino após o login: **Home** (`/`), com placeholder honesto nesta entrega.

> A direção visual (paleta, tipografia, layout) não é decisão em aberto desta spec: será
> definida na implementação pela skill `frontend-design`, dentro do contrato de tokens
> semânticos e do piso de acessibilidade estabelecidos pela skill `frontend-app`.
