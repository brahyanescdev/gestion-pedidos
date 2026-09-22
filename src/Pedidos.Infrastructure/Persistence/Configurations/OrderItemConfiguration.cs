using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(item => item.OrderId)
            .HasColumnName("order_id");

        builder.Property(item => item.ProductId)
            .HasColumnName("product_id");

        builder.Property(item => item.Quantity)
            .HasColumnName("quantity");

        builder.Property(item => item.UnitPrice)
            .HasColumnName("unit_price")
            .HasPrecision(18, 2);

        builder.Ignore(item => item.Subtotal);

        builder.HasIndex(item => item.OrderId);
        builder.HasIndex(item => item.ProductId);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
