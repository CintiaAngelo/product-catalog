using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Infrastructure.Cache;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;

namespace ProductCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("ProductCatalogDb"));

        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "ProductCatalog:";
            });
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
