🎬 Fase 3 — API Pública (Catálogo)
Data: 27/10/2025

Camadas: Movix.Api (REST) · Application (consultas + filtros) · Infrastructure (EF Core / SQL Server)

🎯 Objetivo

Disponibilizar endpoints públicos de catálogo com filtros, paginação, Swagger para documentação automática e CORS configurado por ambiente.
Esta fase consolida a API para consultas externas, garantindo organização, segurança básica e boas práticas de arquitetura.

🧩 Escopo e Implementações

1️⃣ Application (Movix.Application)
Criada a interface IFilmeQueryService, responsável por consultas filtradas e paginadas.
Definidos DTOs e filtros (FilmeDto, FilmeFilter, PagedResult) para padronizar as respostas e parâmetros da API.
Implementada a injeção de dependência dos serviços de consulta, isolando a lógica de negócio da camada de infraestrutura.

2️⃣ Infrastructure (Movix.Infrastructure)
Implementado o serviço FilmeQueryService, que executa consultas com filtros dinâmicos e paginação via repositório EF Core.
Mantido o repositório IFilmeRepository e suas operações assíncronas.
Registrados serviços e repositórios no container de Dependency Injection.
Garantida compatibilidade total com o banco configurado na Fase 1.

3️⃣ API (Movix.Api)
Configurado o Program.cs com:
AddInfrastructure(builder.Configuration) para integrar DbContext, Identity e serviços.
Controllers habilitados para endpoints RESTful.
Swagger e SwaggerUI ativados automaticamente em ambiente de desenvolvimento.
CORS definido via appsettings.Development.json, permitindo apenas origens confiáveis (localhost).
Endpoint / de health-check exibindo { "name": "Movix.Api", "status": "ok" }.

4️⃣ Swagger e Testes
Acesso local confirmado via Swagger UI (http://localhost:5193/swagger).
Testado o endpoint /api/filmes com filtros e paginação, retornando dados conforme esperado.
Verificado funcionamento do health-check e documentação da API.

📡 Endpoints Entregues

GET /api/filmes
Lista filmes com filtros e paginação.

Parâmetros de consulta (opcionais):
generoId — filtra por gênero
classificacaoId — filtra por classificação
ano — filtra por ano
q — busca por título ou sinopse
sortBy — ordenação (CreatedAt, Titulo, Ano)
desc — ordenação decrescente (padrão: true)
page — número da página (padrão: 1)
pageSize — itens por página (padrão: 12, máx. 48)
Resposta (200):
Retorna objeto com paginação, total de itens e lista de filmes (DTO).

✅ Entregáveis / Checklist
 API pública funcional (Movix.Api)
 Endpoint /api/filmes com filtros e paginação
 Swagger configurado e acessível
 CORS ativo apenas em ambiente de desenvolvimento
 Health-check retornando status "ok"
 Serviços e repositórios registrados no DI
 Execução testada com sucesso via dotnet run


🧠 Observações Técnicas
Mantida a arquitetura Clean Architecture (camadas isoladas e bem definidas).
Infraestrutura desacoplada, permitindo troca futura de banco de dados sem afetar a lógica.
Consultas assíncronas com EF Core e LINQ otimizadas.
Swagger documenta automaticamente todos os endpoints, facilitando o consumo externo.
CORS configurável no appsettings, garantindo segurança entre ambientes.
API pronta para integração com o front-end (Movix.Web).

