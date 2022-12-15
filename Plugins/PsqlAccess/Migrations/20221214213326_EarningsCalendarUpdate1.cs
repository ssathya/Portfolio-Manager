using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class EarningsCalendarUpdate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NextRefreshDate",
                table: "earningsCalendars",
                newName: "RemoveDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RemoveDate",
                table: "earningsCalendars",
                newName: "NextRefreshDate");
        }
    }
}
