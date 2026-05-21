using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE UNIQUE INDEX IX_department_locations_department_id_location_id ON department_locations (department_id, location_id)");
            
            migrationBuilder.Sql("CREATE INDEX IDX_departments_path ON departments USING GIST (path);");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_department_locations_department_id_location_id");
            
            migrationBuilder.Sql("DROP INDEX IDX_departments_path;");

        }
    }
}
