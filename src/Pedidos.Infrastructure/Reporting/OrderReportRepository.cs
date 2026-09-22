using Npgsql;
using Pedidos.Application.Dtos;
using Pedidos.Application.Ports;

namespace Pedidos.Infrastructure.Reporting;

public class OrderReportRepository : IOrderReportRepository
{
    private readonly string _connectionString;

    public OrderReportRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<OrderSummaryDto?> GetCustomerOrderSummaryAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM get_customer_order_summary(@customerId)";
        command.Parameters.AddWithValue("customerId", customerId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new OrderSummaryDto(
            reader.GetGuid(reader.GetOrdinal("customer_id")),
            (int)reader.GetInt64(reader.GetOrdinal("total_orders")),
            reader.GetDecimal(reader.GetOrdinal("total_spent")),
            reader.IsDBNull(reader.GetOrdinal("last_order_date")) ? null : reader.GetDateTime(reader.GetOrdinal("last_order_date")));
    }
}
