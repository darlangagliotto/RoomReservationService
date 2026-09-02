# Skills do projeto

Skills versionadas junto com o código: valem para qualquer pessoa (ou agente) que
abrir este repositório.

## Estrutura

Uma pasta por skill, sempre com `SKILL.md` dentro. O **nome da pasta é o nome da
skill** — é o que se digita como `/nome-da-skill`.

```
.claude/skills/
├── README.md                    (este arquivo — não é uma skill)
├── nova-feature/
│   └── SKILL.md
├── novo-endpoint/
│   ├── SKILL.md
│   └── references/
│       └── checklist.md
└── nova-migration/
    └── SKILL.md
```

Nome em `kebab-case`, verbo ou substantivo curto. Arquivos auxiliares (templates,
checklists, scripts) ficam dentro da pasta da skill e são referenciados por caminho
relativo a partir do `SKILL.md`.

## Anatomia do `SKILL.md`

```markdown
---
name: novo-endpoint
description: Cria um endpoint novo em um serviço seguindo Clean Architecture. Use quando o pedido for adicionar rota, action de controller ou caso de uso a AuthService, UserService, RoomService ou ReservationService.
---

# Novo endpoint

## Quando usar
...

## Passos
1. ...
2. ...

## Verificação
- [ ] ...
```

Só `name` e `description` são obrigatórios no frontmatter.

**A `description` é a parte que mais importa.** É o único texto que o modelo lê para
decidir se a skill se aplica — o corpo só é carregado depois que ela dispara. Escreva
em duas partes: *o que faz* + *quando usar*, com as palavras que a pessoa realmente
usaria no pedido ("endpoint", "rota", "migration", "caso de uso", nomes dos serviços).
Descrição vaga = skill que nunca dispara.

O corpo deve ser **procedimento, não teoria**: passos numerados, caminhos de arquivo
reais, comandos prontos para rodar, e um checklist de verificação no fim. Contexto
conceitual já está em `docs/` — referencie com link em vez de repetir.

## Onde colocar cada skill

| Local | Escopo | Use para |
|---|---|---|
| `.claude/skills/` (aqui) | este repositório, versionado | fluxos deste projeto: SDD, endpoint, migration, revisão de camadas |
| `~/.claude/skills/` | seu usuário, todos os projetos | preferências pessoais e hábitos que não são deste projeto |

Na dúvida, aqui. Skill de projeto acompanha o código e vale para todo mundo.

## Como usar

- `/nome-da-skill` invoca explicitamente.
- Sem digitar nada, a skill dispara sozinha quando o pedido casa com a `description`.

## Candidatas para este projeto

Derivadas de [docs/sdd/workflow.md](../../docs/sdd/workflow.md) e das convenções:

| Skill | O que automatiza |
|---|---|
| `nova-spec` | Cria `docs/specs/NNN-nome.md` a partir do template, numera e registra no índice |
| `novo-endpoint` | Gera os 5 arquivos de caso de uso + registro no DI + action no controller + rota no Gateway |
| `novo-servico` | Scaffold dos 4 projetos, Dockerfile, entrada no Compose e no `.slnx` |
| `nova-migration` | `dotnet ef migrations add` com os `--project`/`--startup-project` corretos por serviço |
| `revisar-camadas` | Verifica violações da regra de dependência e do padrão de caso de uso |
| `fechar-spec` | Roda o Definition of Done, atualiza `docs/` e remove o item do backlog |

Comece por `nova-spec` e `novo-endpoint` — são os dois passos que você vai repetir em
toda entrega do backlog.
