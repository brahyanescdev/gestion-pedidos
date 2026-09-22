using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pedidos.Domain.Entities;

namespace Pedidos.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(order => order.CustomerId)
            .HasColumnName("customer_id");

        builder.Property(order => order.OrderDate)
            .HasColumnName("order_date");

        builder.Property(order => order.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Ignore(order => order.Total);

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(order => order.CustomerId);
        builder.HasIndex(order => order.OrderDate);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(order => order.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
