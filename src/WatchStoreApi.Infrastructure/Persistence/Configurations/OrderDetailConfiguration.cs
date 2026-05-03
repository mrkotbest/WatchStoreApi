using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchStoreApi.Domain.Entities;

namespace WatchStoreApi.Infrastructure.Persistence.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.HasKey(od => od.Id);

        builder.Property(od => od.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(od => od.TotalAmount)
            .HasPrecision(18, 2);
    }
}
