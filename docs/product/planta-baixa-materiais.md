# Levantamento — materiais para a planta do andar (spec 009)

Pedido em [information-architecture.md](information-architecture.md#pendências-de-produção):
o que existe pronto na internet para as duas peças de arte que a spec 009 precisa antes de
começar — o **render base** do andar e os **glifos** de equipamento. Isto não é spec, não
gera código, e não decide nada por conta própria: é a base pra quem decide (Darlan) escolher
um caminho.

> **Nota posterior**: a decisão final divergiu da Opção A abaixo. Em vez de um render único
> do andar inteiro com hotspots por sala, o caminho escolhido foi gerar 3 ilustrações de
> sala individuais (mesmo gerador de imagem, prompt ajustado para top-down por sala) e
> distribuí-las entre os cards da grade — ver
> [information-architecture.md](information-architecture.md#histórico-da-decisão-visual) e
> [spec 009](../specs/009-planta-do-andar.md). O raciocínio sobre geradores de imagem e
> bibliotecas de ícone abaixo continua válido; só a unidade de composição mudou de "andar
> inteiro" para "uma sala por imagem".

## 1. Render base do andar (item crítico)

Requisito, conforme [information-architecture.md](information-architecture.md#tratamento-visual):
acabamento realista — piso, mobiliário e volume desenhados —, visto de cima, feito **uma
vez**. Três caminhos, em ordem de esforço:

### Opção A — Geração por IA de imagem (mais rápido para prototipar)

**Gemini "Nano Banana" / Nano Banana Pro** (Google) é hoje a opção mais citada
especificamente para render arquitetônico top-down: mantém proporções e elementos de uma
planta de entrada, e a versão Pro lida bem com texto legível dentro da imagem (útil se
algum rótulo entrar na própria arte). Existe também a linha "floor-plan-to-render" de
serviços dedicados (Rendair, ArchiGPT, Ideal House, LightX) — mesma ideia, empacotada com
UI própria e créditos grátis limitados.

Como usar na prática: descrever a planta em texto (formato do imóvel, número de salas,
estilo — "escritório corporativo moderno, piso laminado claro, luz natural") e pedir
explicitamente "vista de cima, 2D/isométrica, sem pessoas, sem texto sobre a imagem,
render arquitetônico realista". Iterar em cima do resultado costuma ser mais rápido que
começar do zero numa ferramenta 3D.

Prós: grátis ou muito barato, iteração em minutos, não exige aprender ferramenta nova.
Contras: menos controle fino sobre a posição exata de cada sala — a planta pode não bater
perfeitamente com um layout de 10 salas específico, exigindo re-gerar ou editar depois.
Confirmar os termos de uso comercial vigentes do gerador escolhido antes de publicar.

### Opção B — Ferramenta de design 3D com export de render

**Homestyler** e **Planner 5D** permitem desenhar a planta real (paredes, dimensões) e
gerar um render 3D top-down com iluminação e material realistas. Os planos gratuitos de
ambos permitem isso, mas **aplicam marca d'água** no render — que inviabiliza uso direto
num produto. Um plano pago pontual (não assinatura) costuma resolver, se o custo for
aceitável para uma peça de arte feita uma vez só.

Prós: controle total sobre o layout (bate exatamente com as salas reais); resultado mais
prosível de repetir se precisar ajustar depois. Contras: marca d'água no grátis; curva de
aprendizado da ferramenta; ainda exige desenhar a planta manualmente.

### Opção C — Encomendar (fora de "pronto na internet", citado para registro)

Freelancer de arquitetura/3D (Fiverr, Upwork, 99designs) entrega o render sob medida, sem
marca d'água e com direitos de uso claros no contrato. É a opção mais previsível em
qualidade, mas tem custo e prazo — não é "levantamento de internet", é contratação.

### Recomendação prática

Começar pela **Opção A** para validar o conceito (a spec 009 só precisa de uma imagem, um
`Room.PlanSlot` por polígono e a interação por cima) — é reversível e barato errar. Se o
resultado não convencer visualmente ou não bater com o layout real das salas, migrar para
B ou C. Trocar a base depois não exige mudar código (ver information-architecture.md,
"Tratamento visual") — só o arquivo e os polígonos.

## 2. Marcadores dos 11 tipos de equipamento

Requisito: glifos **monocromáticos**, na tinta das paredes — ou seja, ícones de linha
simples, não ilustrações coloridas. Isso favorece bibliotecas de ícones de interface (não
assets de jogo) porque já nascem no estilo certo.

| Tipo (`EquipmentType`) | Cobertura em bibliotecas livres | Observação |
|---|---|---|
| `Tv` | Boa — "tv" existe em Lucide, Phosphor e Material Symbols | — |
| `Monitor` | Boa — "monitor" nas três | — |
| `Notebook` | Boa — "laptop" nas três | — |
| `Telefone` | Boa — "phone" nas três | — |
| `Cadeira` | Boa — "armchair"/"chair" em Lucide e Phosphor | — |
| `ArCondicionado` | Parcial — aproximar com "air-vent"/"fan" (Lucide) ou "ac_unit" (Material Symbols, que tem o ícone literal) | Material Symbols é o mais direto aqui |
| `QuadroBranco` | Parcial — aproximar com "presentation" (Lucide/Phosphor) ou "co_present"/"cast" (Material Symbols) | Nenhum é um whiteboard literal |
| `Projetor` | Fraca — sem ícone de projetor dedicado nas bibliotecas verificadas | Candidato a desenho customizado |
| `Dock` | Fraca a boa — Material Symbols tem um ícone chamado literalmente "dock"; Lucide/Phosphor não têm equivalente claro | Verificar o Material Symbols primeiro |
| `Flipchart` | Fraca — sem ícone dedicado | Candidato a desenho customizado |
| `Outro` | N/A | Precisa de um glifo genérico próprio (ex.: ponto de interrogação ou caixa) |

Bibliotecas recomendadas, todas com licença permissiva (uso comercial livre, sem
atribuição obrigatória):

- **Material Symbols** (Google, Apache 2.0) — o catálogo mais amplo dos três; provável
  melhor cobertura para os tipos "fracos" da tabela acima.
- **Lucide** (ISC) e **Phosphor** (MIT) — estilo de linha mais fino e consistente entre si,
  bom para os tipos com cobertura "boa".

Para os 3-4 tipos sem ícone exato (`Projetor`, `Flipchart`, possivelmente `Dock` e
`QuadroBranco` dependendo da fonte escolhida), desenhar um glifo customizado é barato: são
formas geométricas simples (ex.: projetor = trapézio + círculo de lente; flipchart =
retângulo sobre dois pés). Um único traço monocromático em SVG, no mesmo grid das outras
(24×24), mantém a disciplina visual do documento sem depender de banco de assets.

**Evitar** pacotes de assets de jogo (itch.io, GameDevMarket, CGTrader) para este uso: são
desenhados para cenário de jogo (coloridos, com sombra, estilo pixel-art ou 3D), não para
glifo de interface — e as licenças variam muito por pacote, algumas restringem uso fora de
jogos. Servem de referência visual, não de arquivo pronto para usar aqui.

## 3. Mapear os polígonos (hotspots) sobre o render

Não exige ferramenta nova: qualquer editor que leia coordenadas de pixel sobre uma imagem
resolve — Figma (grátis) ou Photopea (grátis, no navegador, sem conta) bastam para marcar
os quatro cantos de cada uma das até 10 salas e anotar as coordenadas. Isso vira dado
(polígono por `PlanSlot`), não código; a spec 009 consome esse dado, não o produz.

## Resumo para decisão

| Pergunta | Resposta curta |
|---|---|
| Dá para gerar o render sem pagar nada? | Sim, via IA de imagem (Opção A) — com ressalva de controle fino sobre o layout exato |
| Dá para ter os 11 glifos sem pagar nada? | Sim — Material Symbols cobre a maioria; 2 a 4 tipos exigem um desenho customizado simples |
| Alguma licença é um risco real? | Só se usar assets de jogo (itch.io/GameDevMarket) fora dos termos deles. As bibliotecas de ícones recomendadas e os geradores de IA citados têm uso comercial permitido nos termos correntes — confirmar a versão vigente antes de publicar |
| O que falta decidir, fora deste levantamento? | Escolher entre Opção A/B/C para o render (decisão do Darlan) e aprovar visualmente o resultado antes de travar a spec 009 |
