using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class EarningsCalendarUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_earningsCalendars",
                table: "earningsCalendars");

            migrationBuilder.RenameTable(
                name: "earningsCalendars",
                newName: "EarningsCalendars");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EarningsCalendars",
                table: "EarningsCalendars",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EarningsCalExceptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    Exception = table.Column<int>(type: "integer", nullable: false),
                    ReportingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdditionalNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EarningsCalExceptions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EarningsCalExceptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EarningsCalendars",
                table: "EarningsCalendars");

            migrationBuilder.RenameTable(
                name: "EarningsCalendars",
                newName: "earningsCalendars");

            migrationBuilder.AddPrimaryKey(
                name: "PK_earningsCalendars",
                table: "earningsCalendars",
                column: "Id");
        }
    }
}
