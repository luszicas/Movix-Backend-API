🎬 Fase 4 — Web (Catálogo + Filtros, One-Page)
Data: 27/10/2025

Camadas: Movix.Web (front-end Razor/ASP.NET) · Movix.Api (consumido via HttpClient) · Application/Infrastructure (reuso total)

🎯 Objetivo

Entregar a camada Web consumindo a API: renderizar catálogo de filmes com cards, filtros (gênero, classificação, ano, texto), ordenação e paginação.
Unificar a navegação em uma página (one-page) com seções Home, About, Genres e Catalog, mantendo compatibilidade com rotas antigas.


🧩 Escopo e Implementações


1️⃣ Web / Models (Movix.Web.Models)
Criados DTOs e ViewModels: FilmeDto, PagedResult<T>, CatalogFilterVm, CatalogPageVm.
Padronização de dados recebidos da API e parâmetros de filtro/paginação.

2️⃣ Web / Services (Movix.Web.Services)
Criada interface IApiClient e implementação ApiClient.
Métodos principais:
GetGenerosAsync() → lista de gêneros (cache 10min).
GetClassificacoesAsync() → lista de classificações (cache 10min).
SearchFilmesAsync(CatalogFilterVm) → consulta filtrada/paginada à API.
GetFilmeAsync(int id) → detalhes do filme.
HttpClient tipado configurado via DI, usando ApiBaseUrl do appsettings.

3️⃣ Web / Controllers (Movix.Web.Controllers)
HomeController.Index([FromQuery] CatalogFilterVm filter, string? section) → centraliza renderização das seções one-page.
Rotas compatíveis: /Home/Catalog e /Home/Genre → mapeiam para a mesma action e rolam para a seção correta.
Sanitização de filtros/paginação (Page ≥ 1, PageSize ≤ 48).

4️⃣ Web / Views
_Layout.cshtml → navbar com âncoras #home, #about, #genres, #catalog e footer com site map.
Index.cshtml → seções: Home, About, Genres (cards), Catalog (filtros + cards).
_CatalogSection.cshtml → partial view reutilizável para filtros, cards e paginação.
Ajuste Razor para evitar C# direto em atributos (<option selected> via condicionais) → resolve erro RZ1031.
Cards exibem sinopse e informações principais (gênero, classificação, ano).
Paginação visual mostrando página atual, total de páginas e total de itens.

5️⃣ Configuração (Movix.Web/Program.cs)
MVC + MemoryCache + HttpClient tipado.
JSON options configurados (PropertyNameCaseInsensitive, ReadCommentHandling.Skip).
EF Core + Identity (mesmo DbContext da API).
Autenticação via Cookie e policies definidas.
Rotas de área e padrão configuradas.

📡 Funcionalidades testadas / Critérios de aceite
GET /api/filmes → filtros, ordenação e paginação funcionando.
GET /api/filmes/{id} → retorna 200 ou 404 conforme existência.
GET /api/generos e /api/classificacoes → listas retornadas e cache funcionando.
Cards e filtros renderizam corretamente.
Navbar e footer funcional, one-page navegável.
Swagger da API acessível (https://localhost:5193/swagger).
CORS configurado conforme ambiente.

✅ Entregáveis / Checklist
Movix.Web consumindo API via HttpClient tipado.
Partial _CatalogSection com filtros, cards e paginação.
HomeController unificado, compatível com rotas antigas.
Views renderizando corretamente: cards, filtros, navegação one-page.
Cache de gêneros/classificações ativo.
Paginação e ordenação testadas.
Front-end testado via Visual Studio + dotnet run.

🧠 Observações Técnicas
One-page melhora UX e mantém compatibilidade com rotas antigas.
Cache leve reduz chamadas repetidas à API.
Razor sem C# em atributos evita erros de compilação.
Consultas e DTOs alinhados com API para evitar sobrecarga e melhorar performance.
Arquitetura limpa mantida, front-end desacoplado da API, permitindo futuras evoluções.
