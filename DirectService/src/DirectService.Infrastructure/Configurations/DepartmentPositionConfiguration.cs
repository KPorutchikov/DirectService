using DirectService.Domain.Departments;
using DirectService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectService.Infrastructure.Configurations;

public class DepartmentPositionConfiguration : IEntityTypeConfiguration<DepartmentPosition>
{
    public void Configure(EntityTypeBuilder<DepartmentPosition> builder)
    {
        builder.ToTable("department_positions");
        
        builder.HasKey(x => x.Id).HasName("id_department_positions");
        
        builder.Property(x => x.Id).HasColumnName("id");
        
        builder
            .HasOne(d => d.Department)
            .WithMany(p => p.Positions)
            .HasForeignKey("department_id")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne<Position>()
            .WithMany()
            .HasForeignKey(d => d.PositionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.Property(x => x.PositionId).HasColumnName("position_id");

        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");
    }
}