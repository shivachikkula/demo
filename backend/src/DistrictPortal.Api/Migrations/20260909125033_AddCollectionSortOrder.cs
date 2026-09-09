using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistrictPortal.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCollectionSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Collections",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Collections");
        }
    }
}
