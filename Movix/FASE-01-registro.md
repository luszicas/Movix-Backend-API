Fase 1 — Domínio + EF Core (SQL Server) + Migrations + Seed

Data: 24/10/2025

Objetivo
Modelar as entidades do domínio, configurar EF Core e Identity, criar o banco e popular dados iniciais de forma idempotente.

Escopo


1️⃣ Domain/Entities (Movix.Domain)
Filme
Id, Titulo, Sinopse, Ano, ImagemCapaUrl, CreatedAt
FK: Genero, Classificacao
Genero
Id, Nome, coleção de Filmes
Classificacao
Id, Nome, Descricao, coleção de Filmes
TipoUsuario
Id, Descricao (1=Admin, 2=Gerente, 3=Outros)

Nota: O usuário do Identity (ApplicationUser) fica na camada Infrastructure para manter o domínio limpo.

2️⃣ Infrastructure (Movix.Infrastructure)

2.1 Entidade de Identity
ApplicationUser : IdentityUser<Guid>
Campos extras: IsActive, TipoUsuarioId, CreatedAt, UpdatedA
2.2 DbContext
AppDbContext
Fluent API para tabelas, relacionamentos e índices
Índices: Filme(Titulo), Filme(Ano), Filme(GeneroId, ClassificacaoId)
Relacionamentos: Filme:N Genero, Filme:N Classificacao com DeleteBehavior.Restrict
Filtro global: ApplicationUser.IsActive = true

2.3 Seed idempotente (DatabaseSeeder)
Criação de TiposUsuario, Roles, Usuários padrão, Gêneros, Classificações e Filmes iniciais
Inserção condicional para não duplicar dados

2.4 Dependency Injection (DependencyInjection.cs)
Registra:
DbContext
Identity
Repositórios: IFilmeRepository, IGeneroRepository, IClassificacaoRepository
Serviços: IFilmeQueryService, ICatalogLookupService

3️⃣ API (Movix.Api)

3.1 Configuração
appsettings.Development.json com string de conexão LocalDB:
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MovixDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}


3.2 Program.cs
Registra infra (DbContext + Identity + DI)
Ativa controllers, Swagger e CORS
Chama DatabaseSeeder.EnsureSeededAsync(app.Services) para popular dados iniciais


4️⃣ Migration & Banco

Criar Migration
dotnet ef migrations add InitialCreate -p Movix.Infrastructure -s Movix.Api -o Persistence/Migration

Aplicar Migration

dotnet ef database update -p Movix.Infrastructure -s Movix.Api
Caso o banco já exista, escolha outro nome ou remova o banco antes de atualizar.
Rodar API
dotnet run --project Movix.Api
Health endpoint: http://localhost:5000/ → { "name": "Movix.Api", "status": "ok" }
erifique tabelas e dados no SQL Server


5️⃣ Entregáveis / Checklist
 Banco MovixDb criado no LocalDB
 Tabelas AspNetUsers, AspNetRoles, Filmes, Generos, Classificacoes, TiposUsuario
 Dados seed inseridos corretamente
 Endpoint health retorna status ok
 Repositórios e serviços registrados no DI
 Identity configurado com Guid e filtro IsActive = true

6️⃣ Observações técnicas
Fluent API mantém domínio limpo (sem DataAnnotations)
Seeds idempotentes permitem reiniciar API sem duplicar dados
DeleteBehavior.Restrict evita deleções acidentais em FK
GUID no Identity garante consistência de usuários
