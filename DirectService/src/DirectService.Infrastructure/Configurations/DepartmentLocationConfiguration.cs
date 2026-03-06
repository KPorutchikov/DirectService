using DirectService.Domain.Departments;
using DirectService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectService.Infrastructure.Configurations;

public class DepartmentLocationConfiguration : IEntityTypeConfiguration<DepartmentLocation>
{
    public void Configure(EntityTypeBuilder<DepartmentLocation> builder)
    {
        builder.ToTable("department_locations");

        builder.HasKey(x => x.Id).HasName("id_department_locations");
        
        builder.Property(x => x.Id).HasColumnName("id");

        builder
            .HasOne(d => d.Department)
            .WithMany(l => l.Locations)
            .HasForeignKey("department_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne<Location>()
            .WithMany()
            .HasForeignKey(l => l.LocationId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.LocationId).HasColumnName("location_id");
        
        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");
    }
}