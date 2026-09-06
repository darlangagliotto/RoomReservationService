# 007 — Atribuir equipamentos às salas

| Campo | Valor |
|---|---|
| Status | Rascunho |
| Serviços afetados | **RoomService** (endpoints novos), Frontend |
| Depende de | [005](005-navegacao-e-salas.md) (tela de salas), [006](006-cadastro-de-equipamentos.md) (há o que atribuir) |
| Autor / data | Darlan · 2026-09-06 |

## 1. Problema

**A API não permite mudar os equipamentos de uma sala depois que ela existe.**

`POST /api/rooms` aceita `equipmentIds` no cadastro e é a única porta: `PATCH /api/rooms/{id}`
altera nome, número e `planSlot`, e explicitamente **não toca em equipamentos**. O domínio
já sabe fazer — `Room.AddEquipment` e `Room.RemoveEquipment` existem e validam —, mas
nenhum caso de uso e nenhum controller os chamam. Está registrado como lacuna em
[room-service.md](../services/room-service.md#lacunas-conhecidas) e é o motivo de a spec 005
ter deixado "alterar equipamentos de uma sala existente" fora de escopo.

Na prática: comprou um projetor depois de cadastrar a sala? Ele nunca entra nela. Mudou a TV
de andar? A alocação antiga fica mentindo para sempre. Como a planta desenha os marcadores
a partir dessa alocação, o erro é visível na tela principal do produto.

Esta é a **única das três specs deste bloco que exige código de backend**.

## 2. Resultado esperado

Do ponto de vista de quem consome a API: dá para adicionar um equipamento livre a uma sala
existente e para retirá-lo, com as mesmas garantias do cadastro — um equipamento em no
máximo uma sala, sem duplicata dentro da sala.

Do ponto de vista de quem usa o produto: na tela de Salas, a pessoa abre uma sala e gerencia
o que há dentro dela — adiciona de uma lista de equipamentos livres, retira o que não está
mais lá — e a mudança aparece imediatamente, sem recadastrar a sala.

## 3. Escopo

**Incluído**

- `POST /api/rooms/{id}/equipments` — aloca um ou mais equipamentos livres à sala
- `DELETE /api/rooms/{id}/equipments/{equipmentId}` — desaloca um equipamento da sala
- Casos de uso `AssignEquipmentsToRoomUseCase` e `RemoveEquipmentFromRoomUseCase`
- Testes unitários dos dois casos de uso e das invariantes já existentes no `Room`
- Frontend: painel de gestão de equipamentos da sala, a partir da tela `/salas`
- Atualização de [room-service.md](../services/room-service.md) e da lacuna correspondente

**Fora de escopo**

- Estender `PATCH /api/rooms/{id}` para aceitar `equipmentIds`. Ver §5 — é decisão tomada,
  não omissão.
- Editar ou remover o equipamento em si — a API continua sem esses endpoints (spec 006, §3)
- Mover equipamento de uma sala direto para outra em uma chamada: faz-se com `DELETE` e `POST`
- Posição do equipamento dentro da sala — continua derivada do tipo pela âncora semântica
  ([information-architecture.md](../product/information-architecture.md#derivar-não-pedir))
- A planta — spec 009

## 4. Contrato

Dois endpoints novos no `RoomController`. Roteamento no Gateway: **já coberto** por
`/api/rooms/{**catch-all}`; nenhuma rota nova é necessária.

### `POST /api/rooms/{id:guid}/equipments` — autenticado

Request:
```json
{ "equipmentIds": ["<guid>", "<guid>"] }
```

`200 OK` com o `RoomResponse` completo e atualizado, no mesmo envelope do `PATCH`:
```json
{ "room": { "id": "…", "name": "Sala Azul", "number": 101, "planSlot": 3,
            "equipments": [ … ] } }
```

Devolver a sala inteira, e não só o que mudou, deixa o cliente reidratar a tela com uma
resposta só — é o que o `PATCH` já faz e não há razão para divergir.

Erros (`400` `ProblemDetails`, `title: "Erro de negócio"`):

| Condição | `detail` |
|---|---|
| Sala inexistente | `"Sala não encontrada."` |
| Lista vazia ou ausente | `"Informe ao menos um equipamento."` |
| `Guid.Empty` na lista | `"Há equipamento com identificador inválido."` |
| Equipamento inexistente | `"Equipamento não encontrado."` |
| Equipamento já alocado a **outra** sala | `"Este equipamento já está alocado a outra sala."` |
| Equipamento já nesta sala | `"Este equipamento já está na sala."` (`DomainException`) |

### `DELETE /api/rooms/{id:guid}/equipments/{equipmentId:guid}` — autenticado

Sem corpo. `200 OK` com o `RoomResponse` atualizado, mesmo envelope.

| Condição | `detail` |
|---|---|
| Sala inexistente | `"Sala não encontrada."` |
| Equipamento não está nesta sala | `"Este equipamento não está na sala."` (`DomainException`) |

Desalocar **não apaga** o equipamento: ele volta a aparecer em
`GET /api/equipments?unassigned=true`.

## 5. Regras de negócio

### Invariantes — já existem em `Room`, não reescrever

`AddEquipment` recusa `Guid.Empty` e duplicata dentro da sala; `RemoveEquipment` recusa
equipamento ausente. Ver
[domain/model.md](../domain/model.md#room--raiz-roomservice). Esta spec **não altera o
domínio** — só constrói os casos de uso e a porta HTTP que faltavam.

### Regras de aplicação — nos casos de uso

1. **Um equipamento, uma sala.** Antes de alocar, verificar se o equipamento já pertence a
   outra sala. É a mesma regra que `RegisterRoomUseCase` aplica no cadastro, e reaproveitar
   a verificação existente é preferível a duplicá-la.
2. **Validar tudo antes de gravar qualquer coisa.** Com uma lista de ids, a operação é
   atômica: se um único equipamento falhar, nada é alocado e a mensagem identifica o
   problema. Alocação parcial deixaria o cliente sem saber o que aconteceu.
3. **A ordem de verificação é a da tabela de erros** (§4) — a mesma disciplina de
   `CreateReservationUseCase`.

### Por que não estender o `PATCH`

`PATCH /api/rooms/{id}` tem semântica de campo: manda-se o que mudou, e o que não vem fica
como está. Equipamentos são uma **coleção**, e coleção em `PATCH` é ambígua — a lista
enviada substitui, acrescenta, ou remove o que não veio? Qualquer resposta a essa pergunta
precisa ser documentada e lembrada, e a errada silenciosamente apaga alocação de outra
pessoa.

Endpoints explícitos de alocação e desalocação não têm essa ambiguidade: cada chamada diz o
que faz, cada erro aponta um equipamento, e duas pessoas mexendo na mesma sala não se
sobrescrevem. O custo é uma chamada por remoção — irrelevante numa sala com poucos itens.

Mesma razão para **não** haver um `PUT` que troca o conjunto inteiro.

### Frontend

O painel vive na tela **Salas** (`/salas`), não na de Equipamentos. A pergunta que a pessoa
faz é "o que tem nesta sala?", e a resposta pertence à sala. A tela de Equipamentos (spec
006) continua mostrando a alocação como **leitura** — ela responde "onde está este
equipamento?", que é outra pergunta.

O seletor oferece apenas `GET /api/equipments?unassigned=true`: um equipamento já alocado
não aparece como opção, então o erro de alocação dupla vira caminho excepcional (corrida
entre duas pessoas), não fluxo normal. Quando ele acontecer, a mensagem do servidor é
exibida como está e a lista é recarregada.

## 6. Impacto em dados

**Nenhuma migration.** A tabela de junção `RoomEquipment` já existe com a FK e a chave
necessárias ([domain/model.md](../domain/model.md#roomdb)); esta spec apenas passa a inserir
e remover linhas nela por uma porta nova.

Dados existentes: intocados. A operação é reversível — desalocar devolve o equipamento ao
conjunto de livres, sem perda de informação.

> Fica registrado, **fora do escopo**, que a unicidade "um equipamento em no máximo uma
> sala" continua garantida apenas em código, sem índice único no banco
> ([B5](../sdd/backlog.md)). Esta spec não piora a situação, mas passa a exercitá-la com
> mais frequência: com duas portas de alocação em vez de uma, a janela de corrida aparece
> mais. Corrigir isso é spec própria.

## 7. Impacto entre serviços

Nenhum serviço novo é chamado. O RoomService continua sem chamar ninguém.

**Ordem de implantação**: o RoomService precisa subir com os endpoints novos antes de o
frontend correspondente ir ao ar. Como os dois sobem pelo mesmo `docker compose`, na prática
é uma entrega só; se forem separadas, backend primeiro — o inverso deixa botão que responde
`404`.

O ReservationService consome `GET /api/rooms/{id}` e continua funcionando sem alteração; ele
não olha equipamentos.

## 8. Critérios de aceite

**Backend**

- [ ] Dado uma sala existente e um equipamento livre, quando faço `POST /api/rooms/{id}/equipments`, então recebo `200` com a sala contendo o equipamento.
- [ ] Dado um equipamento já alocado a outra sala, quando tento alocá-lo, então recebo `400` com `"Este equipamento já está alocado a outra sala."` e **nada** é gravado.
- [ ] Dado uma lista com um id válido e um inexistente, então **nenhum** dos dois é alocado e a resposta aponta o inexistente.
- [ ] Dado um equipamento que já está na sala, quando tento alocá-lo de novo, então recebo `400` com `"Este equipamento já está na sala."`.
- [ ] Dado uma sala inexistente, então recebo `400` com `"Sala não encontrada."`.
- [ ] Dado um equipamento alocado, quando faço `DELETE`, então recebo `200` sem ele na sala e ele volta a aparecer em `GET /api/equipments?unassigned=true`.
- [ ] Dado um equipamento que não está na sala, quando faço `DELETE`, então recebo `400` com `"Este equipamento não está na sala."`.
- [ ] Dado que não envio token, então recebo `401` nos dois endpoints.
- [ ] O `RoomResponse` devolvido pelos dois endpoints traz `equipments` preenchido com `placement` — sem regressão do `Include` corrigido na spec 002.
- [ ] Testes unitários cobrem os dois casos de uso, incluindo o caminho de falha atômica.

**Frontend**

- [ ] Dado uma sala na lista, então consigo abrir a gestão de equipamentos dela.
- [ ] Dado o painel aberto, então vejo os equipamentos da sala e uma seleção contendo **apenas** equipamentos livres.
- [ ] Dado que adiciono um equipamento, então ele aparece na sala e some da lista de livres, sem eu recarregar a página.
- [ ] Dado que retiro um equipamento, então ele sai da sala e volta à lista de livres.
- [ ] Dado que o servidor recusa a alocação, então vejo a mensagem dele junto ao painel e a lista é recarregada.
- [ ] Em 375px o painel é usável e não produz scroll horizontal.
- [ ] O painel é percorrível apenas com teclado, com foco visível, e fecha com `Esc`.
- [ ] `npx tsc --noEmit` limpo e `npm test` passando.

## 9. Decisões em aberto

Nenhuma. As duas que existiam foram resolvidas em §5: forma do contrato (endpoints
explícitos, não `PATCH` nem `PUT`) e onde o painel vive (tela de Salas).
