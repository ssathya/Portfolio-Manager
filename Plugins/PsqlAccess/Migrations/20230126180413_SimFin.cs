using System;
using System.Collections.Generic;
using ApplicationModels.SimFin;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class SimFin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SimFinRatios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    ProcessingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Indicators = table.Column<List<Indicator>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SimFinRatios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SimFinRatios_Ticker",
                table: "SimFinRatios",
                column: "Ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SimFinRatios");
        }
    }
}
