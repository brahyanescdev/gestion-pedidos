using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pedidos.Application.Ports;
using Pedidos.Infrastructure.Persistence;
using Pedidos.Infrastructure.Persistence.Repositories;
using Pedidos.Infrastructure.Reporting;

namespace Pedidos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PedidosDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOrderReportRepository>(_ => new OrderReportRepository(connectionString));

        return services;
    }
}
