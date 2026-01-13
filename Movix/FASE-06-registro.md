🎬 Fase 6 — Dashboard Administrativo + Gestão (placeholders)
Data: 28/10/2025
Camadas: Movix.Web (Área Admin + MVC) · integra Movix.Infrastructure (AppDbContext / Identity)

🎯 Objetivo

Entregar o Dashboard administrativo (cards, gráficos e lista de últimos filmes) para Administradores e a seção de Gestão (placeholders dos CRUDs) acessível a Gerentes e Administradores.

Nesta fase, os CRUDs ainda não são implementados — apenas as páginas de entrada e visualização básica.



🧩 Escopo e Implementações


1️⃣ Dashboard (AdminOnly)
Rota: /Admin/Dashboard
Acesso: somente para usuários com role Admin.
Elementos entregues:
Cards: total de filmes, total de gêneros, total de usuários, total de usuários por perfil.
Gráficos: filmes por gênero, por classificação e por ano (via Chart.js).
Lista: exibe os 5 últimos filmes cadastrados.
Serviço: utiliza consultas diretas no AppDbContext (sem API).
Scripts: gráficos centralizados em wwwroot/js/dashboard.js.

🗂️ Arquivos
Criado: Movix-Web/Areas/Admin/Services/DashboardService.cs

Métodos principais:
- TotalFilmesAsync()
- TotalGenerosAsync()
- TotalUsuariosAsync()
- TotalUsuariosPorRoleAsync()
- FilmesPorGeneroAsync()
- FilmesPorClassificacaoAsync()
- FilmesPorAnoAsync()
- UltimosFilmesAsync(int take = 5)



Controller: Movix.Web/Areas/Admin/Controllers/DashboardController.cs
Decorado com [Authorize(Policy = "AdminOnly")]
Index() preenche dados via ViewBag e renderiza Views/Dashboard/Index.cshtml
Endpoint JSON GET /Admin/Dashboard/Data retorna labels + datasets para os gráficos


View: Movix.Web/Areas/Admin/Views/Dashboard/Index.cshtml
Cards de totais + painel “Usuários por perfil”
Três gráficos:
chartGenero (filmes por gênero)
chartClass (filmes por classificação)
chartAno (filmes por ano)
Lista dos 5 últimos filmes
Importa Chart.js (CDN) e ~/js/dashboard.js


Script: Movix.Web/wwwroot/js/dashboard.js
Faz fetch('/Admin/Dashboard/Data')
Renderiza gráficos com Chart.js (bar e line charts)




2️⃣ Gestão (ManagerOrAdmin)
Rota: /Admin/Manage
Acesso: Admin ou Gerente
Função: página de entrada para os módulos administrativos (placeholders dos CRUDs).

🗂️ Arquivos


Controller: Movix.Web/Areas/Admin/Controllers/ManageController.cs
[Authorize(Policy = "ManagerOrAdmin")]
Ações implementadas:
Index() → menu de entrada para os módulos
Filmes(), Generos(), Classificacoes(), Usuarios() → placeholders vazios


Views criadas:
Movix.Web/Areas/Admin/Views/Manage/Index.cshtml → cartões de navegação
Movix.Web/Areas/Admin/Views/Manage/Filmes.cshtml
Movix.Web/Areas/Admin/Views/Manage/Generos.cshtml
Movix.Web/Areas/Admin/Views/Manage/Classificacoes.cshtml
Movix.Web/Areas/Admin/Views/Manage/Usuarios.cshtml

(todas placeholders aguardando os CRUDs da Fase 7)



3️⃣ Registro de Dependência

No arquivo Movix.Web/Program.cs, foi adicionada a injeção do serviço de dashboard:


using Movix.Web.Areas.Admin.Services;
// ...
builder.Services.AddScoped<DashboardService>();

Nenhuma outra alteração estrutural no pipeline ou nas policies (mantidas da Fase 5).


4️⃣ Ajustes da Área Admin


Atualizado _ViewImports.cshtml da área Admin:


@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@using Movix.Web
@using Movix.Web.Areas.Admin.Services

Rotas já configuradas anteriormente (mantidas da Fase 5):


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");




📊 Funcionalidades Testadas / Critérios de Aceite

Funcionalidade
Status
Observação
/Admin/Dashboard
✅
acessível apenas por Admin
/Admin/Dashboard/Data
✅
retorna JSON para gráficos
/Admin/Manage
✅
acessível por Admin e Gerente
Gráficos Chart.js
✅
exibem dados reais do banco
Últimos filmes
✅
lista 5 últimos cadastros
Placeholders dos CRUDs
✅
layout preparado para Fase 7
Policies
✅
AdminOnly e ManagerOrAdmin respeitadas



✅ Entregáveis / Checklist
 Dashboard completo com cards, gráficos e lista.
 Gestão com navegação para CRUDs (placeholders).
 Serviço de dados (DashboardService) usando AppDbContext.
 Scripts de gráficos centralizados.
 Injeção de dependência configurada.
 Acesso controlado via policies.
 Views responsivas e integradas ao layout principal.



🧠 Observações Técnicas
A camada Web lê diretamente do AppDbContext, evitando dependência da API.
Chart.js foi utilizado por ser leve e fácil de integrar com dados JSON.
Toda a base de autenticação e autorização aproveita a configuração da Fase 5.
Estrutura pronta para receber CRUDs completos na Fase 7, sem retrabalho.
Nenhum CSS inline novo foi adicionado; estilo herdado de Bootstrap e site.css.