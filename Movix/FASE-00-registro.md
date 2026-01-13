🎬 Fase 0 — Scaffold & Fundamentos (Movix)

📅 Data: 24/10/2025

🎯 Objetivo

Criar a solução .NET 9 com 5 projetos, subir a API e o Web mínimos e preparar o terreno para evoluir sem retrabalho.

⚙️ Escopo (entra)

Solução Movix.sln

Projetos:

Movix.Domain

Movix.Application

Movix.Infrastructure

Movix.Api

Movix.Web

Navbar fixa e páginas base no Web:

Home

About

Catalog

Genre

Logout

Endpoint simples na API (GET /) retornando:

{ "name": "Movix.Api", "status": "ok" }

🚫 Escopo (não entra)

Banco de dados, migrations e seed

Autenticação e autorização

Endpoints REST reais (CRUDs)

📂 Entregáveis (arquivos-chave)

Movix.Api

Program.cs com endpoint / e CORS liberado provisoriamente

Movix.Web

Program.cs

Controllers/HomeController.cs

Views/Shared/_Layout.cshtml (navbar fixa + Bootstrap 5.3 via CDN)

Views/Home/*.cshtml (Home, About, Catalog, Genre, Logout)

Raiz

global.json fixando SDK do .NET 9 (ou rollForward ativado)

🧠 Recursos aplicados (por que conta)

Pin de SDK (global.json) → garante que todos usem a mesma versão do .NET

Bootstrap 5.3 via CDN → design moderno e responsivo sem complicações

CORS liberado temporariamente → permite integração entre API e Web durante o desenvolvimento

💻 Como rodar (Windows)
dotnet build
dotnet run --project Movix.Api
# abrir http://localhost:5203/ → { "name": "Movix.Api", "status": "ok" }

dotnet run --project Movix.Web
# abrir a Home com navbar e páginas base

✅ Checklist de conclusão

 Solução Movix.sln criada e compilando

 API respondendo { "name": "Movix.Api", "status": "ok" }

 Web abrindo Home com navbar fixa

 Páginas About, Catalog, Genre e Logout criadas

 Estrutura pronta para a Fase 1 (entidades e EF Core)