using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class EarningsCalendarAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "earningsCalendars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    EarningsDateYahoo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VendorEarningsDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EarningsReadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextRefreshDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataObtained = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_earningsCalendars", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "earningsCalendars");
        }
    }
}
