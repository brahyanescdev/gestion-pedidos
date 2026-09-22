using Pedidos.Api.Middleware;
using Pedidos.Application.UseCases;
using Pedidos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PedidosDb")
    ?? throw new InvalidOperationException("La cadena de conexión 'PedidosDb' no está configurada.");

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");
app.MapControllers();

app.Run();

public partial class Program
{
}
