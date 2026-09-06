# Arquitetura de informação

Mapa do produto: que telas existem, o que cada uma resolve, como se navega entre elas e
**o que cada uma exige do backend que ainda não existe**. Não é spec — não gera código.
É a referência que as specs de tela consultam para não inventar navegação por acidente.

## Para quem, e para quê

Funcionário de escritório que precisa de uma sala para uma reunião. O trabalho central do
produto é **achar uma sala livre no horário que eu quero e reservá-la**. Todo o resto —
cadastro de salas, de equipamentos, gestão das minhas reservas — existe para sustentar
esse ato.

Consequência de projeto: a tela inicial não é um painel de métricas. É a própria busca.

## Decisões tomadas

| Decisão | Razão |
|---|---|
| **Um andar só, planta fixa, no máximo 10 salas** | O andar existe e não muda; isso permite uma planta desenhada uma vez |
| **Acabamento realista, não esquemático** | Requisito do produto. Ver "Tratamento visual" |
| **Três estados**: disponível · reservada · em uso | Sala livre agora mas reservada logo não serve para uma reunião longa |
| **Status é derivado das reservas**, nunca um campo | Uma sala não "é" ocupada; ela está ocupada num intervalo |
| **A referência de tempo abre em "agora" e é móvel** | Abrir em agora é útil sem configurar; sem poder mover, não se reserva para mais tarde |
| **Navegação no topo**, não lateral | 4 seções não justificam menu lateral, e a planta precisa de toda a largura |
| **Derivar, não pedir** | Posição pedida no cadastro vira campo vazio em produção. Ver abaixo |

## Tratamento visual

**Requisito registrado**: a planta deve ter acabamento **realista** — piso, mobiliário e
volume desenhados —, não traço esquemático. Planta em linha fina foi avaliada e
**recusada**. A referência aceita é um render arquitetônico visto de cima.

Como a planta é **fixa** e tem no máximo 10 salas, o caminho é:

| Camada | O que é | Origem |
|---|---|---|
| **Base** | Render realista do andar inteiro, visto de cima | Feito **uma vez**, fora do sistema (ferramenta 3D ou arte encomendada) |
| **Hotspots** | Um polígono invisível por sala, sobre a base | `Room.PlanSlot` liga a sala ao polígono |
| **Estado** | Etiqueta com texto e contorno, por sala | Calculado pela API de disponibilidade |
| **Equipamentos** | Marcadores nas âncoras + lista no painel | Dados do cadastro |

Consequência a assumir de olhos abertos: **o mobiliário do render é cenário**. O
equipamento *cadastrado* aparece como marcador sobre o render e na lista do painel — não
como móvel desenhado. Trocar a base (render novo) não exige mudar código, só o arquivo e
os polígonos.

O estado **nunca** é uma lavagem de cor sobre a sala: com piso desenhado, a película briga
com a arte. Etiqueta com texto e contorno, como na referência.

Como a planta é fixa, o modelo **não precisa** de largura, profundidade nem formato da
sala. Isso foi removido do escopo.

### Derivar, não pedir

Princípio que governa a posição dos objetos:

- **Posição do objeto na sala** é derivada do **tipo**, via âncora semântica:

| Âncora | Tipos | Desenho |
|---|---|---|
| `parede` | TV, monitor, tela | encostado numa parede |
| `teto` | projetor, ar-condicionado | marcado no centro |
| `mesa` | notebook, telefone, dock | sobre a mesa central |
| `piso` | flipchart, cadeira extra | canto livre |

Em ambos os casos o modelo guarda um campo de sobrescrita **nulo**. Precisão vira aditiva
depois, sem migração dolorosa — e provavelmente ninguém vai precisar. **Não** construir
editor de arrastar-e-soltar antes de alguém provar que a posição exata importa.

## Navegação

Barra no topo, dentro do `AppShell` que já existe. Quatro destinos:

```
Room Reservation     Planta   Reservas   Salas   Equipamentos     usuario@email  Sair
```

| Rota | Seção | Resolve |
|---|---|---|
| `/` | **Planta** | achar sala livre e reservar (Home) |
| `/salas/:id` | — (detalhe) | avaliar uma sala e ver sua agenda |
| `/reservas` | **Reservas** | ver e cancelar o que reservei |
| `/salas` | **Salas** | cadastrar e editar salas |
| `/equipamentos` | **Equipamentos** | cadastrar equipamentos |

A barra lista apenas destinos que existem. Hoje: Início e Salas (spec 005); Equipamentos,
Reservas e Planta entram com as specs 006, 008 e 009. Em telas estreitas a barra quebra para a
linha de baixo em vez de colapsar num menu — com poucos itens, um hambúrguer esconderia
mais do que ajudaria.

> **Sem papéis.** O JWT não carrega papel e a autorização do sistema é binária: qualquer
> pessoa autenticada cadastra e edita salas e equipamentos. Isso é consequência do backend
> atual, não escolha de produto — e deve virar decisão consciente antes de qualquer uso
> real.

## As telas

### Planta do andar — `/` (Home)

Vista de cima do andar. Cada sala é desenhada como planta (parede com vão de porta e arco
de abertura), com o número em numeral tabular ao centro e os equipamentos como glifos de
traço fino nas suas âncoras.

- **Estado** por lavagem de cor **mais etiqueta**: teal = livre, âmbar = ocupada, sempre
  com texto ("Livre", "Ocupada até 15:30"). Nunca só cor.
- **Linha do tempo acima da planta**: arrastar move a referência e **repinta o andar**.
  É o elemento de assinatura da tela e o que a torna ferramenta de reserva, não diagrama.
- **Passar o mouse** eleva a sala; **clicar ou tocar** abre painel com equipamentos e
  próximos horários. O painel é o caminho garantido; o hover é enriquecimento.
- **Abaixo de 768px** a planta dá lugar a cartões de sala com exatamente as mesmas
  etiquetas e a mesma lista. Não é degradação: é a apresentação certa para o espaço.

Disciplina de cor: glifos de equipamento são monocromáticos, na tinta das paredes. **Cor
é exclusiva do status** — caso contrário a planta vira confete e o status deixa de saltar.

### Detalhe da sala — `/salas/:id`

A sala em tamanho grande, com todos os equipamentos e a agenda do dia. É a tela de decidir
("essa serve?") e de reservar com intervalo preciso.

### Nova reserva — painel sobre a Planta ou o Detalhe

Não é rota própria: nasce do contexto em que a pessoa já está, com sala e horário
pré-preenchidos pelo que ela estava olhando. Datas convertidas para UTC na saída.

### Reservas — `/reservas`

Lista das minhas reservas, com cancelamento. Sem endpoint de "minhas reservas", filtra-se
por `userId` — lido do claim `sub` do token.

### Salas — `/salas` · Equipamentos — `/equipamentos`

Listagem e cadastro. Telas de dados densos: tabela acima de 768px, cartões abaixo. É onde
o `400` de lista vazia precisa virar estado vazio, não erro.

## O que cada tela exige do backend

A coluna da direita é o valor deste documento: nenhuma tela deve ser especificada sem
saber o que falta abaixo dela.

| Tela | Backend | Situação |
|---|---|---|
| Planta | `GET /api/reservations/availability`, `GET /api/rooms` | ✅ pronto — falta o **render** |
| Detalhe da sala | `GET /api/rooms/{id}`, `GET /api/reservations?roomId=` | ✅ pronto |
| Nova reserva | `POST /api/reservations` | ✅ pronto |
| Reservas | `GET`, `DELETE /api/reservations`, `GET /api/users/{id}` | ✅ pronto |
| Equipamentos de uma sala | `POST`/`DELETE /api/rooms/{id}/equipments` | ❌ **não existe** — spec 007 |
| Salas | `GET`, `POST`, `PATCH /api/rooms` | ✅ pronto e **em uso** (spec 005) |
| Equipamentos | `GET`, `POST /api/equipments` | ✅ pronto |

**O backend quase deixou de ser o gargalo.** As três lacunas estruturais que este documento
levantou foram fechadas pelas specs 002, 003 e 004: consulta por id com propagação de
token, disponibilidade por intervalo com os três estados, e vocabulário de equipamento com
âncoras mais `PlanSlot`. Restou uma, descoberta ao escrever a spec 007: **não há como
alterar os equipamentos de uma sala depois de criada** — o domínio sabe, a API não expõe.

O que trava a planta agora é **produção de arte**, não código — ver "Pendências de
produção" abaixo.

## Sequência de specs

| # | Spec | Bloco | Depende | Estado |
|---|---|---|---|---|
| 002 | Consulta por id e autenticação serviço-a-serviço | backend | — | ✅ implementada |
| 003 | Disponibilidade por intervalo, com três estados | backend | 002 | ✅ implementada |
| 004 | Catálogo de equipamentos e vínculo com a planta | backend | — | ✅ implementada |
| 005 | **Navegação no topo** + Salas: lista, cadastro, edição | frontend | — | implementada |
| 006 | Cadastro de equipamentos | frontend | 004, 005 | **escrita — desbloqueada** |
| 007 | Atribuir equipamentos às salas | **backend** + frontend | 005, 006 | **escrita — exige endpoints novos** |
| 008 | Reservar salas: criar, listar, cancelar | frontend | 002, 005 | **escrita — desbloqueada** |
| 009 | Planta do andar com estado e linha do tempo | frontend | 003, 004, 008, **render** | a escrever |
| 010 | Detalhe da sala | frontend | 004, 009 | a escrever |

**A 005 não depende de backend nenhum.** `GET`, `POST` e `PATCH /api/rooms` já existem e
funcionam; o `planSlot` da spec 004 entra depois como campo adicional. Ou seja: a barra de
navegação e a primeira tela de dados podem ser construídas **em paralelo** com o bloco de
backend, não depois dele.

A navegação nasce junto da 005 porque é ali que existe o primeiro destino real. Barra de
menu antes disso apontaria para lugar nenhum.

## Pendências de produção

Itens que não são código e têm prazo de entrega próprio. A spec 009 não começa sem eles.

| Item | Quem | Situação |
|---|---|---|
| Render do andar visto de cima, realista, planta fixa, até 10 salas | Fora do sistema — ferramenta 3D ou arte encomendada | **pendente, caminho crítico** |
| Marcadores dos tipos de equipamento (11 tipos da spec 004) | idem | pendente |
| Levantamento do que dá para obter pronto na internet — ferramentas, pacotes de assets, licenças | Claude, a pedido | **a entregar** |

O levantamento acima foi pedido para "quando chegar nos desenhos", mas o render está no
caminho crítico da 009: convém antecipá-lo.

Duas observações sobre a ordem:

- A **005 é o seguro**: entrega o sistema funcionando — cadastrar, listar, reservar — sem
  depender da planta. Se a planta atrasar ou mudar de forma, ainda há produto.
- A **004 é independente da 002 e 003** e pode correr em paralelo.

## Adiado conscientemente

Múltiplos andares · editor de posição por arrastar · capacidade da sala · papéis e
permissões · reserva recorrente · convidados numa reserva.

Cada um destes é conceito de domínio que não existe. Nenhum deve ser introduzido de
improviso dentro de uma spec de tela — precisa de decisão própria.
