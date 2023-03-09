using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSmaToMomMfDolAvg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SmA",
                table: "MomMfDolAvgs",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SmA",
                table: "MomMfDolAvgs");
        }
    }
}