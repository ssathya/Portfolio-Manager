using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIndexNameTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdxName",
                table: "IndexComponents",
                newName: "ListedInIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ListedInIndex",
                table: "IndexComponents",
                newName: "IdxName");
        }
    }
}
