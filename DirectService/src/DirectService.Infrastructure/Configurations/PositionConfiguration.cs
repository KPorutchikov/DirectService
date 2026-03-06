using DirectService.Domain;
using DirectService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectService.Infrastructure.Configurations;

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("positions");
        
        builder.HasKey(x => x.Id).HasName("id_positions");
        
        builder.Property(x => x.Id).HasColumnName("id");
        
        builder.Property(x => x.Name).HasMaxLength(LengthConstants.Length100).IsRequired().HasColumnName("name");
        
        builder.Property(x => x.Description).IsRequired(false).HasColumnName("description");
        
        builder.Property(x => x.IsActive).IsRequired().HasColumnName("is_active");
        
        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");
        
        builder.Property(x => x.UpdatedAt).IsRequired(false).HasColumnName("updated_at");
    }
}