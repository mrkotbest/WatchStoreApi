using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WatchStoreApi.Domain.Entities;

namespace WatchStoreApi.Infrastructure.Persistence.Configurations;

public class ShoppingCartItemConfiguration : IEntityTypeConfiguration<ShoppingCartItem>
{
    public void Configure(EntityTypeBuilder<ShoppingCartItem> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(s => s.TotalAmount)
            .HasPrecision(18, 2);

        builder.HasOne(s => s.User)
            .WithMany(u => u.ShoppingCartItems)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
