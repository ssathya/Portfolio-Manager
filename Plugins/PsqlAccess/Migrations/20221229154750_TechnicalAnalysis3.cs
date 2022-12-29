using System;
using System.Collections.Generic;
using ApplicationModels.Compute;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class TechnicalAnalysis3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Momentums");

            migrationBuilder.DropTable(
                name: "YQuotes");

            migrationBuilder.CreateTable(
                name: "Computes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    ComputedValues = table.Column<List<ComputedValues>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Computes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Computes_Ticker",
                table: "Computes",
                column: "Ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Computes");

            migrationBuilder.CreateTable(
                name: "Momentums",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ComputedValues = table.Column<List<ComputedValues>>(type: "jsonb", nullable: false),
                    Ticker = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Momentums", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "YQuotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Close = table.Column<decimal>(type: "numeric", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    High = table.Column<decimal>(type: "numeric", nullable: false),
                    Low = table.Column<decimal>(type: "numeric", nullable: false),
                    Open = table.Column<decimal>(type: "numeric", nullable: false),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    Volume = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YQuotes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Momentums_Ticker",
                table: "Momentums",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_YQuotes_Ticker",
                table: "YQuotes",
                column: "Ticker");
        }
    }
}
