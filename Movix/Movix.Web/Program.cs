using System.Text.Json;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Movix.Infrastructure.Entities;         // ApplicationUser
using Movix.Web.Services;
using Movix.Web.Areas.Admin.Services;
using Movix.Infrastructure.Persistence;       // AppDbContext
using Movix.Infrastructure.Persistence.Seed; // DatabaseSeeder

var builder = WebApplication.CreateBuilder(args);

// MVC + cache + HttpClient p/ API
builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
var baseUrl = builder.Configuration["ApiBaseUrl"]
?? throw new InvalidOperationException("ApiBaseUrl não configurada.");
client.BaseAddress = new Uri(baseUrl);
});

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
    o.SerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
});

// ========== EF CORE ==========
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection não configurada.");

builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

// ========== Identity com Roles ==========
builder.Services
    .AddIdentityCore<ApplicationUser>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        opt.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager();

// Cookies
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
})
.AddCookie(IdentityConstants.ApplicationScheme, opt =>
{
    opt.LoginPath = "/Auth/Login";
    opt.LogoutPath = "/Auth/Logout";
    opt.AccessDeniedPath = "/Auth/Denied";
    opt.SlidingExpiration = true;
});

// Policies
builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminOnly", policy => policy.RequireAssertion(ctx =>
         ctx.User.IsInRole("ADMIN") ||
         ctx.User.IsInRole("Admin") ||
         ctx.User.IsInRole("Administrador")
     ));

    opt.AddPolicy("ManagerOrAdmin", policy => policy.RequireAssertion(ctx =>
        ctx.User.IsInRole("ADMIN") ||
        ctx.User.IsInRole("Admin") ||
        ctx.User.IsInRole("Administrador") ||
        ctx.User.IsInRole("GERENTE") ||
        ctx.User.IsInRole("Gerente")
    ));
});

// Services do Dashboard
builder.Services.AddScoped<DashboardService>();

var app = builder.Build();

// ⬇⬇⬇ SEED DO BANCO (migra + popula) ⬇⬇⬇
using (var scope = app.Services.CreateScope())
{
    await DatabaseSeeder.EnsureSeededAsync(scope.ServiceProvider);
}
// ⬆⬆⬆ OBRIGATÓRIO PARA POPULAR O BANCO ⬆⬆⬆

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Rotas de Áreas
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
);

// Rota padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();
