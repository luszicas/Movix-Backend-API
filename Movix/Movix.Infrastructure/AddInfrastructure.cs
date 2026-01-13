using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Movix.Application.Abstractions.Repositories;
using Movix.Application.Services;
using Movix.Infrastructure.Persistence;
using Movix.Infrastructure.Repositories;
using Movix.Infrastructure.Services;

namespace Movix.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // DbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(config.GetConnectionString("DefaultConnection")));

            // Repositórios
            services.AddScoped<IFilmeRepository, EfFilmeRepository>();
            services.AddScoped<IGeneroRepository, EfGeneroRepository>();
            services.AddScoped<IClassificacaoRepository, EfClassificacaoRepository>();

            // Serviços
            services.AddScoped<IFilmeQueryService, FilmeQueryService>();
            services.AddScoped<ICatalogLookupService, CatalogLookupService>();

            return services;
        }
    }
}
