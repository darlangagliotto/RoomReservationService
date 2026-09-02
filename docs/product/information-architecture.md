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
| **Um andar só** | Prédio multi-andar é conceito adiado; entra quando houver demanda real |
| **Status é derivado das reservas**, nunca um campo | Uma sala não "é" ocupada; ela está ocupada num intervalo |
| **A referência de tempo abre em "agora" e é móvel** | Abrir em agora é útil sem configurar; sem poder mover, não se reserva para mais tarde |
| **Navegação no topo**, não lateral | 4 seções não justificam menu lateral, e a planta precisa de toda a largura |
| **Derivar, não pedir** | Posição pedida no cadastro vira campo vazio em produção. Ver abaixo |

### Derivar, não pedir

Princípio que governa as duas decisões espaciais do produto:

- **Posição da sala no andar** é derivada do **número**. Prédios reais numeram ao longo de
  corredores; distribuir as salas por ordem numérica em duas fileiras produz uma planta
  plausível com zero cadastro.
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

Abaixo de 768px a barra colapsa; acima, os quatro rótulos ficam visíveis.

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

| Tela | Já existe | **Falta no backend** |
|---|---|---|
| Planta | `GET /api/rooms` | disponibilidade por intervalo; tipo de equipamento como vocabulário; dimensão da sala |
| Detalhe da sala | `GET /api/reservations?roomId=` | `GET /api/rooms/id/{id}` ([B1](../sdd/backlog.md#b1)); âncora por tipo |
| Nova reserva | `POST /api/reservations` | B1 — hoje falha sempre com `"User not found."` |
| Reservas | `GET`, `DELETE /api/reservations` | B1, para o nome do usuário |
| Salas | `GET`, `POST`, `PATCH /api/rooms` | — |
| Equipamentos | `POST /api/equipments` | **listagem de equipamentos não existe**; e `/api/equipments` **não é roteado pelo Gateway** |

Três lacunas são estruturais e aparecem em mais de uma tela:

1. **B1** — endpoints por id e autenticação serviço-a-serviço. Trava reserva inteira.
2. **Disponibilidade** — estado derivado por intervalo. Sem isso não há planta com status.
3. **Vocabulário de tipo + geometria** — sem isso o desenho não passa de retângulo.

## Sequência de specs

| # | Spec | Bloco | Depende |
|---|---|---|---|
| 002 | B1: endpoints por id + auth serviço-a-serviço | backend | — |
| 003 | Disponibilidade por intervalo | backend | 002 |
| 004 | Geometria, vocabulário de tipo e âncoras; listar equipamentos; rota do Gateway | backend | — |
| 005 | Navegação no topo + telas de lista e cadastro | frontend | 002, 004 |
| 006 | Planta do andar com status e linha do tempo | frontend | 003, 004, 005 |
| 007 | Detalhe da sala | frontend | 004, 006 |

Duas observações sobre a ordem:

- A **005 é o seguro**: entrega o sistema funcionando — cadastrar, listar, reservar — sem
  depender da planta. Se a planta atrasar ou mudar de forma, ainda há produto.
- A **004 é independente da 002 e 003** e pode correr em paralelo.

## Adiado conscientemente

Múltiplos andares · editor de posição por arrastar · capacidade da sala · papéis e
permissões · reserva recorrente · convidados numa reserva.

Cada um destes é conceito de domínio que não existe. Nenhum deve ser introduzido de
improviso dentro de uma spec de tela — precisa de decisão própria.
