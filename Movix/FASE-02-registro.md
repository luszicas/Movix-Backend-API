# Fase 2 — Repositórios & Serviços (filtros + paginação) (data: 26/10/2025)

**Objetivo**  
Padronizar contratos (DTOs/filtros), criar repositórios EF e serviços de consulta com filtros/ordenação/paginação e registrar no DI.

## Escopo (entra)
- **DTOs (records / classes)**:
  - `PagedRequest` (classe com Page/PageSize saneados)
  - `PagedResult<T>` (record imutável com metadados de paginação)
  - `FilmeDto` (DTO enxuto para listagem de filmes)
- **Filtro**:
  - `FilmeFilter : PagedRequest` (classe com filtros opcionais: `GeneroId?`, `ClassificacaoId?`, `Ano?`, `Q`, `SortBy`, `Desc`, `Page`, `PageSize`)
- **Repositórios (Application / Abstractions)**:
  - `IFilmeRepository` — pesquisa filmes com filtros/paginação
  - `IGeneroRepository` — lista de gêneros
  - `IClassificacaoRepository` — lista de classificações
- **Infra (implementação EF)**:
  - `EfFilmeRepository` — inclui `AsNoTracking`, `Include`, `EF.Functions.Like`, ordenação, paginação
  - `EfGeneroRepository`, `EfClassificacaoRepository` — leitura simples com `AsNoTracking` e ordenação
- **Serviços (Application / Infra)**:
  - `IFilmeQueryService` + `FilmeQueryService` — orquestra filtros e projeta para `FilmeDto`
  - `ICatalogLookupService` + `CatalogLookupService` — retorna tuplas `(Id, Nome)` para combos/dropdowns
- **DI**:
  - Registro de repositórios e serviços concretos em `AddInfrastructure(...)` no projeto `Movix.Infrastructure`

## Detalhes Técnicos / Entregáveis
- **Ordenação**: suportado `"Titulo" | "Ano" | "CreatedAt"`, padrão `CreatedAt DESC`
- **Busca textual**: campo `Q` busca em `Titulo` e `Sinopse` usando `LIKE` (`%termo%`) seguro contra SQL Injection
- **Paginação**: `Page>=1`; `PageSize` saneado para `[1..48]`, padrão 12
- **Projeção**: `FilmeQueryService` retorna `PagedResult<FilmeDto>` com apenas os dados necessários para a UI
- **Performance**: consultas usam `AsNoTracking`; `Include` apenas para `Genero` e `Classificacao`

## Observações importantes
- DTOs e filtros imutáveis ajudam a manter contratos claros entre camadas
- Contratos no Application permitem testar com mocks sem precisar do EF/Core
- Limitar `PageSize` previne requisições pesadas e abuso de API
- Navegações EF (`Include`) são otimizadas para evitar N+1
- Serviços projetam entidades de domínio para DTOs, evitando vazamento do domínio para a UI

## Estrutura resumida
- `Domain`: entidades puras (`Filme`, `Genero`, `Classificacao`, `TipoUsuario`)
- `Application`: DTOs, filtros, interfaces de repositório/serviço
- `Infrastructure`: EF Core, repositórios, serviços concretos, DI
- `Api`: composição das camadas e endpoints (Fase 3)
- `Web`: UI e chamadas à API (Fases 4–6)

## Como validar / smoke test
```bat
dotnet build
dotnet run --project Movix.Api
:: Build deve passar sem erros, DI resolve repositórios e serviços
:: Endpoints ainda não implementados, foco em build e DI funcionando
