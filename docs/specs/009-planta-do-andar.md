# 009 — Planta do andar: grade de cards com estado e painel de detalhe

| Campo | Valor |
|---|---|
| Status | Implementada |
| Serviços afetados | Frontend (ReservationService e RoomService consumidos, sem alteração) |
| Depende de | [003](003-disponibilidade-de-salas.md) (estado por intervalo), [004](004-catalogo-de-equipamentos-e-planta.md) (equipamentos), [008](008-reservar-salas.md) (formulário de reserva reaproveitado) |
| Autor / data | Darlan · 2026-09-09 |

## 1. Problema

A tela inicial (`/`) hoje é um placeholder ("Nada por aqui ainda"). É o trabalho central do
produto — achar uma sala livre agora e reservá-la — e não existe interface para isso; a
única forma de saber se uma sala está livre é abrir `/reservas` e cruzar horários na cabeça.

## 2. Resultado esperado

Uma pessoa autenticada abre a Home e vê, para cada sala cadastrada, se ela está livre,
reservada ou em uso agora. Clica numa sala e vê o detalhe — equipamentos, status,
próximo horário — num painel lateral, sem sair da tela. Do painel, reserva a sala com um
clique a mais, com o horário já sugerido.

## 3. Escopo

**Incluído**

- `/` (Home) — grade de cards, um por sala cadastrada
- Cada card: imagem ilustrativa de fundo, nome, número, bolinha de estado com texto
- Estado calculado contra uma janela fixa de referência: agora até agora + 30 minutos
- Painel lateral ao clicar num card: equipamentos, status, próximo horário, botão de
  reserva
- Botão de reserva abre o mesmo painel de nova reserva da spec 008, com a sala já
  selecionada
- Os quatro estados de tela: carregando, vazio, erro, sucesso

**Fora de escopo**

- Linha do tempo arrastável para mover a janela de referência — a Home sempre mostra
  "agora". Registrado como extensão futura, não decisão permanente.
- Planta única do andar com hotspots posicionados (`Room.PlanSlot`) — abandonada; ver
  [information-architecture.md](../product/information-architecture.md#histórico-da-decisão-visual)
- Rota de detalhe própria (`/salas/:id`) — o painel lateral cobre a necessidade imediata;
  spec 010 decide se uma rota dedicada vale a pena depois
- Imagem por sala individual (foto real) — as 3 imagens são um pool fixo, reaproveitado
- Filtro por andar, capacidade ou equipamento — não existem esses conceitos no domínio

## 4. Contrato

Nenhum endpoint novo. Consome, pelo Gateway:

| Chamada | Uso |
|---|---|
| `GET /api/reservations/availability?start=&end=` | status de cada sala na janela de referência |
| `GET /api/rooms` | nome, número, equipamentos de cada sala (para o painel de detalhe) |
| `POST /api/reservations` | reservar a partir do painel (spec 008, sem alteração) |

Ambos os `GET` já têm cliente pronto no frontend (`api/reservations.ts`, `api/rooms.ts`).
`GET /api/reservations/availability` é chamado com `start = agora` e `end = agora + 30min`,
convertidos para UTC pelo módulo único de data (`lib/datetime.ts`, spec 008).

## 5. Regras de negócio

Nenhuma regra nova de domínio. Regras de apresentação:

### Distribuição das imagens

Três imagens fixas em `Frontend/public/rooms/` (`sala-1.jpg`, `sala-2.jpg`, `sala-3.jpg`),
geradas uma vez fora do sistema. Cada sala recebe uma delas de forma **determinística** —
a mesma sala sempre mostra a mesma imagem entre uma navegação e outra — via uma função
pura que deriva o índice do `id` da sala (hash simples do GUID `mod 3`). Não é aleatório a
cada render: isso trocaria a imagem da sala a cada re-fetch da lista e pareceria bug.

### Estado do card

Mapeamento direto da resposta da spec 003, sempre bolinha (`●`) mais texto — nunca só cor:

| `status` da API | Texto | Cor (token) |
|---|---|---|
| `Disponivel` | "Livre" | `--color-success` |
| `Reservada` | "Reservada" | `--color-accent` |
| `EmUso` | "Em uso" | `--color-muted` |

### Painel de detalhe

Ao clicar num card, abre um painel lateral com: nome e número da sala, a mesma bolinha de
estado, equipamentos cadastrados (`room.equipments`, já vem no `GET /api/rooms`), e o
campo de contexto relevante ao estado — `busyUntil` quando `EmUso`, `nextReservationAt`
quando `Reservada`, nada quando `Disponivel`. O botão "Reservar esta sala" abre o painel de
nova reserva da spec 008 com `initialRoomId` preenchido; a spec 008 já foi construída para
receber isso (ver `ReservationForm`, comentário "a mesma peça é reaberta pela planta").

### Janela de referência fixa

"Agora" muda a cada segundo; fixar em "agora" literal na hora do fetch já responde à
pergunta que importa ("essa sala serve pra uma reunião que eu preciso começar já"). Uma
janela de 30 minutos (não um instante) evita que uma sala prestes a começar uma reserva
aaproxime demais do limite de arredondamento de datas. Mover essa janela fica para quando
o produto pedir de verdade — não antes.

## 6. Impacto em dados

Nenhum. Frontend puro — nenhuma entidade, coluna ou migration. `Room.PlanSlot` continua
existindo no schema (spec 004), mas sem uso funcional nesta tela.

## 7. Impacto entre serviços

Nenhum contrato novo. A Home passa a chamar `GET /api/reservations/availability` e
`GET /api/rooms` na mesma carga — dois `GET`, sem N+1 por sala. Com o RoomService fora do
ar, `GET /api/reservations/availability` falha inteira com `"Não foi possível carregar as
salas."` (comportamento já definido na spec 003) — a tela mostra estado de erro com opção
de repetir, nunca uma grade com salas faltando.

## 8. Critérios de aceite

- [x] Dado que estou autenticado, então a Home mostra um card por sala cadastrada.
- [x] Dado que uma sala tem reserva sobrepondo a janela `[agora, agora+30min)`, então o card mostra "● Em uso".
- [x] Dado que uma sala não tem reserva na janela mas tem uma começando depois, no mesmo dia, então o card mostra "● Reservada".
- [x] Dado que uma sala não se encaixa em nenhum dos dois casos, então o card mostra "● Livre".
- [x] Dado que clico num card, então vejo um painel com equipamentos, status e o horário de contexto certo para aquele status.
- [x] Dado o painel aberto, quando clico em "Reservar esta sala", então o formulário de nova reserva abre com a sala já selecionada.
- [x] Dado nenhuma sala cadastrada, então vejo estado vazio, não erro.
- [x] Dado que a API demora, então vejo skeleton com a forma do conteúdo.
- [x] Dado que a API falha, então vejo estado de erro com opção de repetir.
- [x] Em 375px a grade quebra para coluna única, sem scroll horizontal.
- [x] O fluxo é percorrível apenas com teclado, com foco visível; o painel fecha com `Esc`.
- [x] `npx tsc --noEmit` limpo, sem `any`, e `npm test` passando.

## 9. Decisões em aberto

Nenhuma. A trilha até aqui (planta única → SVG vetorial → grade de cards com imagem por
pool) está registrada em
[information-architecture.md](../product/information-architecture.md#histórico-da-decisão-visual)
para quem for revisitar essa escolha.
