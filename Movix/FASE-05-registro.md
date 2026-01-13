🎬 Fase 5 — Web com Identity (cookies) + Áreas e Policies
Data: 28/10/2025

Camadas: Movix.Web (MVC) · Movix.Infrastructure (DbContext/Identity)

🎯 Objetivo

A camada Web passa a autenticar diretamente com ASP.NET Identity via cookies, usando o mesmo AppDbContext da solução.
O consumo da API continua apenas para o catálogo.

Policies implementadas:
AdminOnly → Admin
ManagerOrAdmin → Gerente ou Admin

🧩 Escopo e Implementações

1️⃣ Configuração e Pacotes
Referências adicionadas:
dotnet add Movix.Web reference Movix.Infrastructure
dotnet add Movix.Web package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add Movix.Web package Microsoft.EntityFrameworkCore.SqlServer


appsettings.Development.json:

{
  "ApiBaseUrl": "http://localhost:5203/",
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MovixDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
EF Core aponta para mesma base da API.
Identity configurado com ApplicationUser e Roles (Guid).
Cookies configurados (LoginPath, LogoutPath, AccessDeniedPath, SlidingExpiration).
Policies definidas para aceitar variações de nomes de roles (Admin, Administrador, Gerente).

2️⃣ Web / Controllers
AuthController:
Login (GET /Auth/Login + POST /Auth/Login)
Logout (POST /Auth/Logout)
AccessDenied (GET /Auth/Denied)
Valida usuário ativo, e-mail único, hash de senha via SignInManager e UserManager.
Redireciona via ReturnUrl seguro ou para /Home/Index.
Área Admin (/Admin):
DashboardController → AdminOnly
Exibe totais de filmes, gêneros, usuários, usuários por role, últimos filmes.
Endpoint JSON /Admin/Dashboard/Data para gráficos.
ManageController → ManagerOrAdmin
Redireciona para CRUDs (filmes, gêneros, classificações, usuários).


3️⃣ Web / Models
LoginVm:
Email, Password, RememberMe, ReturnUrl.
Bind do formulário de login.


4️⃣ Web / Views
_Layout.cshtml
Navbar com links condicionalmente visíveis conforme roles.
Botão Área restrita / Sair (nome) com estado de login.
Toggle light/dark mode.
Auth/Login.cshtml
Formulário completo com AntiForgeryToken, campos de login e remember-me.
Exibe erros de validação.
Admin/Views/Dashboard/Index.cshtml
Cards de totais (filmes, gêneros, usuários, usuários por role).
Gráficos: filmes por gênero, classificação e ano.
Lista dos 5 últimos filmes.
Admin/Views/Manage/Index.cshtml
Painel de navegação para CRUDs (filmes, gêneros, classificações, usuários).


5️⃣ Web / Program.cs
Configuração:
MVC + MemoryCache + HttpClient tipado.
EF Core + IdentityCore + Roles + SignInManager.
Cookies e Policies.
Rotas de áreas (Admin) e padrão.

Pipeline:
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


📡 Funcionalidades testadas / Critérios de aceite
/Auth/Login → login com usuários semeados
Admin: admin@movix.local / Admin@123
Gerente: gerente@movix.local / Gerente@123
Usuário normal: usuario@movix.local / Usuario@123
Login/Logout funcionando, cookie aplicado.
Navbar e menus de área respeitam roles.
/Admin/Dashboard → acessível apenas por Admin (403 outros).
/Admin/Manage → acessível por Gerente ou Admin (403 outros).
Consumo da API para catálogo continua inalterado.


✅ Entregáveis / Checklist
Movix.Web autenticando com Identity + cookies.
AuthController com login, logout, AccessDenied.
Area Admin com Dashboard e Manage, respeitando policies.
Navbar dinâmica conforme estado de login.
Partial views e gráficos do Dashboard funcionando.
Program.cs configurado com EF Core, Identity, Policies e rotas.


🧠 Observações Técnicas
Identity via cookies facilita autenticação sem depender da API.
Policies permitem flexibilidade no controle de acesso.
Front-end desacoplado da API para catálogo, mantendo performance.
Preparado para Fase 6: CRUDs completos e serviços adicionais do Admin.
