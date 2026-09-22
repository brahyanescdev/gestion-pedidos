using Microsoft.EntityFrameworkCore;
using Pedidos.Application.Ports;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly PedidosDbContext _context;

    public CustomerRepository(PedidosDbContext context)
    {
        _context = context;
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Customers.FirstOrDefaultAsync(customer => customer.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Customers.AsNoTracking().ToListAsync(cancellationToken);

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
        => await _context.Customers.AddAsync(customer, cancellationToken);
}
