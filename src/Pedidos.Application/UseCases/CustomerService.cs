using Pedidos.Application.Dtos;
using Pedidos.Application.Ports;
using Pedidos.Domain.Entities;

namespace Pedidos.Application.UseCases;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        var customer = Customer.Create(request.Name, request.Email);
        await _customerRepository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToDto(customer);
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(id, cancellationToken);
        return customer is null ? null : ToDto(customer);
    }

    public async Task<IReadOnlyCollection<CustomerDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _customerRepository.GetAllAsync(cancellationToken);
        return customers.Select(ToDto).ToList();
    }

    private static CustomerDto ToDto(Customer customer) => new(customer.Id, customer.Name, customer.Email);
}
