using Microsoft.AspNetCore.Identity;
using Movix.Infrastructure;
using Movix.Infrastructure.Entities;
using Movix.Infrastructure.Persistence;
using Movix.Infrastructure.Persistence.Seed; // <- adicione no topo do Program.cs













// ...chat


var builder = WebApplication.CreateBuilder(args);

// DI: repositórios e serviços
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();







// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();















// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ??
            new[] { "https://localhost:5001", "http://localhost:5000" })
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ...


// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DevCors");
app.UseAuthorization();

// ✅ Adicione esta linha abaixo:
app.MapGet("/", () => Results.Ok(new { name = "Movix.Api", status = "ok" }));

// Controllers
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DatabaseSeeder.EnsureSeededAsync(services);
}

;
app.Run();
