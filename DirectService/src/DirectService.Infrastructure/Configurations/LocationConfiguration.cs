using DirectService.Domain;
using DirectService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectService.Infrastructure.Configurations;

public class LocationConfiguration : IEntityTypeConfiguration<Location>
{
    public void Configure(EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("locations");
        
        builder.HasKey(x => x.Id).HasName("id_locations");
        
        builder.Property(x => x.Id).HasColumnName("id");

        // LocationName
        builder.OwnsOne(l => l.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length120)
                .HasColumnName("location_name");
        
            nameBuilder.HasIndex(n => n.Value)
                .IsUnique()
                .HasDatabaseName("IX_locations_location_name_unique");
        });
    
        // Address
        builder.OwnsOne(l => l.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length120)
                .HasColumnName("address");
        
            addressBuilder.HasIndex(a => a.Value)
                .IsUnique()
                .HasDatabaseName("IX_locations_address_unique");
        });
    
        // TimeZone
        builder.OwnsOne(l => l.TimeZone, tzBuilder =>
        {
            tzBuilder.Property(t => t.Value)
                .IsRequired()
                .HasColumnName("timezone");
        });
        
        // builder.ComplexProperty(c => c.Name, n =>
        // {
        //     n.Property(p => p.Value)
        //         .IsRequired()
        //         .HasMaxLength(LengthConstants.Length120)
        //         .HasColumnName("location_name");
        // });
        //
        // builder.ComplexProperty(c => c.Address, a =>
        // {
        //     a.Property(p => p.Value)
        //         .IsRequired()
        //         .HasMaxLength(LengthConstants.Length120)
        //         .HasColumnName("address");
        // });
        //
        // builder.ComplexProperty(c => c.TimeZone, t =>
        // {
        //     t.Property(p => p.Value)
        //         .IsRequired()
        //         .HasColumnName("timezone");
        // });
        
        builder.Property(x => x.IsActive).IsRequired().HasColumnName("is_active");
        
        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");
        
        builder.Property(x => x.UpdatedAt).IsRequired(false).HasColumnName("updated_at");
        
    }
}