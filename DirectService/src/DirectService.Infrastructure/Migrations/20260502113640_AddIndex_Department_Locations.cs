using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndex_Department_Locations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE INDEX IX_department_locations_department_id_location_id ON department_locations (department_id, location_id)");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_department_locations_department_id_location_id");

        }
    }
}
