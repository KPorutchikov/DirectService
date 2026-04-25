using DirectService.Domain;
using DirectService.Domain.Departments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DirectService.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        
        builder.HasKey(d => d.Id).HasName("id_departments");
        
        builder.Property(d => d.Id).HasColumnName("id");

        // DepartmentName
        builder.OwnsOne(l => l.DepartmentName, nameBuilder =>
        {
            nameBuilder.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length150)
                .HasColumnName("department_name");
        
            nameBuilder.HasIndex(a => a.Value)
                .IsUnique()
                .HasDatabaseName("IX_departments_name_unique");
        });
        
        // Identifier
        builder.OwnsOne(l => l.Identifier, identifierBuilder =>
        {
            identifierBuilder.Property(a => a.Value)
                .IsRequired()
                .HasMaxLength(LengthConstants.Length150)
                .HasColumnName("identifier");
        
            identifierBuilder.HasIndex(a => a.Value)
                .IsUnique()
                .HasDatabaseName("IX_departments_identifier_unique");
        });
        
        // Path
        builder.ComplexProperty(c => c.Path, p =>
        {
            p.Property(v => v.Value)
                .IsRequired()
                .HasColumnName("path");
        });
        
        builder.Property(x => x.ParentId).IsRequired(false).HasColumnName("parent_id");
        
        builder.Property(x => x.Depth).IsRequired().HasColumnName("depth");
        
        builder.Property(x => x.IsActive).IsRequired().HasColumnName("is_active");
        
        builder.Property(x => x.CreatedAt).IsRequired().HasColumnName("created_at");
        
        builder.Property(x => x.UpdatedAt).IsRequired(false).HasColumnName("updated_at");
    }
}