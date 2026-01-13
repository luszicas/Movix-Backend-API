
🎬 Fase 7 — Gestão (CRUDs + Usuários + Soft Delete)

📅 Data: 28/10/2025

🏗️ Camadas: Movix.Web (Área Admin + MVC) · integra Movix.Infrastructure (AppDbContext / Identity)


🎯 Objetivo
Implementar os CRUDs completos e funcionais de todas as entidades administrativas do sistema:
🎞️ Filmes
🏷️ Gêneros
🔞 Classificações
👥 Usuários (com atribuição de role, soft delete e reativação)

Esta fase consolida o módulo de gestão da área Admin, com regras de segurança, políticas, validações e controle de acesso entre Admin e Gerente.


🧩 Escopo e Implementações

1️⃣ Filmes

📍 Rota: /Admin/Filmes
🔒 Acesso: ManagerOrAdmin

🗂️ Arquivos criados:
Movix.Web/Areas/Admin/Models/FilmeEditVm.cs
Movix.Web/Areas/Admin/Controllers/FilmesController.cs
Movix.Web/Areas/Admin/Views/Filmes/Index.cshtml
Movix.Web/Areas/Admin/Views/Filmes/_Form.cshtml
Movix.Web/Areas/Admin/Views/Filmes/Create.cshtml
Movix.Web/Areas/Admin/Views/Filmes/Edit.cshtml
Movix.Web/Areas/Admin/Views/Filmes/Delete.cshtml

📋 Recursos:
CRUD completo com validação de campos obrigatórios.
Dropdowns para Gênero e Classificação.
Bloqueia exclusão de filmes em uso.
Listagem paginada e filtrada por título.


2️⃣ Gêneros

📍 Rota: /Admin/Generos
🔒 Acesso: ManagerOrAdmin
🗂️ Arquivos criados:
Movix.Web/Areas/Admin/Models/GeneroEditVm.cs
Movix.Web/Areas/Admin/Controllers/GenerosController.cs
Movix.Web/Areas/Admin/Views/Generos/Index.cshtml
Movix.Web/Areas/Admin/Views/Generos/_Form.cshtml
Movix.Web/Areas/Admin/Views/Generos/Create.cshtml
Movix.Web/Areas/Admin/Views/Generos/Edit.cshtml
Movix.Web/Areas/Admin/Views/Generos/Delete.cshtml

📋 Recursos:
CRUD completo com validação de nome.
Bloqueia exclusão se houver filmes associados.
Mensagem de erro amigável ao tentar excluir registros vinculados.


3️⃣ Classificações

📍 Rota: /Admin/Classificacoes
🔒 Acesso: ManagerOrAdmin

🗂️ Arquivos criados:
Movix.Web/Areas/Admin/Models/ClassificacaoEditVm.cs
Movix.Web/Areas/Admin/Controllers/ClassificacoesController.cs
Movix.Web/Areas/Admin/Views/Classificacoes/Index.cshtml
Movix.Web/Areas/Admin/Views/Classificacoes/_Form.cshtml
Movix.Web/Areas/Admin/Views/Classificacoes/Create.cshtml
Movix.Web/Areas/Admin/Views/Classificacoes/Edit.cshtml
Movix.Web/Areas/Admin/Views/Classificacoes/Delete.cshtml

📋 Recursos:
CRUD padrão com campos simples.
Validação de duplicidade e obrigatoriedade.
Exclusão bloqueada se em uso por filmes.


4️⃣ Usuários

📍 Rota: /Admin/Usuarios
🔒 Acesso:
CRUD básico → ManagerOrAdmin
Desativar/Reativar → AdminOnly

🗂️ Arquivos criados:
Movix.Web/Areas/Admin/Models/UsuarioEditVm.cs
Movix.Web/Areas/Admin/Controllers/UsuariosController.cs
Movix.Web/Areas/Admin/Views/Usuarios/Index.cshtml
Movix.Web/Areas/Admin/Views/Usuarios/Create.cshtml
Movix.Web/Areas/Admin/Views/Usuarios/Edit.cshtml


📋 Recursos:
CRUD completo com atribuição de roles (Admin, Gerente, Usuário).
Soft delete: botão “Desativar” marca usuário como inativo.
Reativar: botão visível apenas para Admin.
Filtros: texto, role e status (ativo/inativo).
Gerente não pode desativar/reativar (retorna 403 Forbidden).

5️⃣ Seeds e Cache

🗂️ Arquivos criados/atualizados:
Movix.Web/Views/Shared/CacheKeys.cs
Movix.Infrastructure/Persistence/Seed/DatabaseSeeder.cs

📋 Recursos:
Seeder cria roles e usuários iniciais:
Nome
Email
Senha
Role
Admin
admin@movix.com
123456
Admin
Gerente
gerente@movix.com
123456
Gerente
Usuário
usuario@movix.com
123456
Usuario
Função idempotente: evita duplicações ao rodar novamente.
Usa NormalizedName e NormalizedEmail para comparação consistente.



6️⃣ Políticas de Acesso (Policies)

Configuradas no Program.cs:


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Admin", "Gerente"));
});



Aplicadas nos controllers com [Authorize(Policy = "AdminOnly")] ou [Authorize(Policy = "ManagerOrAdmin")].




7️⃣ Ajustes e Integração

📍 Atualizado ManageController e suas views para apontar para os novos módulos CRUD.
📍 Todas as rotas da Área Admin mantidas:


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");


📍 Nenhum ajuste no pipeline ou middleware.



📊 Funcionalidades Testadas / Critérios de Aceite


Funcionalidade
Status
Observação
Login Admin → /Admin/Manage
✅
acesso completo
Login Gerente → /Admin/Manage
✅
acesso parcial
CRUD de Filmes
✅
completo
CRUD de Gêneros
✅
bloqueia exclusão se houver vínculos
CRUD de Classificações
✅
completo
CRUD de Usuários
✅
com roles e filtros
Soft delete / Reativar
✅
apenas Admin
Filtros (texto, role, status)
✅
funcionais
Policies AdminOnly / ManagerOrAdmin
✅
respeitadas
Seeder idempotente
✅
sem duplicações
Views responsivas
✅
estilo herdado do Bootstrap




✅ Entregáveis / Checklist
 CRUDs completos (Filme, Gênero, Classificação, Usuário)
 Policies configuradas (AdminOnly / ManagerOrAdmin)
 Soft delete implementado (usuário)
 Seeder idempotente
 Layouts responsivos com Bootstrap
 Acesso seguro via Identity
 Gerente sem permissão de desativar/reativar
 Integração com dashboard e área admin
 Testes manuais concluídos com sucesso



🧠 Observações Técnicas
Uso de IgnoreQueryFilters() para listar usuários inativos.
Relacionamentos e validações tratados via EF Core.
Nenhum CSS inline novo — mantém estilo da fase anterior.
Interface e layout seguem o padrão do dashboard da Fase 6.
Estrutura 100% compatível com a futura fase de relatórios (F8).

