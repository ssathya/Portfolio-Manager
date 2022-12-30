using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class TechnicalAnalysis4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoreDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    PiotroskiComputedValue = table.Column<int>(type: "integer", nullable: false),
                    LastEarningsDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SimFinRating = table.Column<int>(type: "integer", nullable: false),
                    ReturnOnAssets = table.Column<bool>(type: "boolean", nullable: false),
                    OperatingCashFlow = table.Column<bool>(type: "boolean", nullable: false),
                    ChangeInROA = table.Column<bool>(type: "boolean", nullable: false),
                    ChangInLongTermDebt = table.Column<bool>(type: "boolean", nullable: false),
                    ReturnOnEquity = table.Column<bool>(type: "boolean", nullable: false),
                    ChangeInNetIncome = table.Column<bool>(type: "boolean", nullable: false),
                    ChangeInCurrentRatio = table.Column<bool>(type: "boolean", nullable: false),
                    ChangeInSharesOutstanding = table.Column<bool>(type: "boolean", nullable: false),
                    GrossMargin = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreDetails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IndexComponents_Ticker",
                table: "IndexComponents",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_FinStatements_Ticker",
                table: "FinStatements",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_EarningsCalendars_Ticker",
                table: "EarningsCalendars",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreDetails_Ticker",
                table: "ScoreDetails",
                column: "Ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoreDetails");

            migrationBuilder.DropIndex(
                name: "IX_IndexComponents_Ticker",
                table: "IndexComponents");

            migrationBuilder.DropIndex(
                name: "IX_FinStatements_Ticker",
                table: "FinStatements");

            migrationBuilder.DropIndex(
                name: "IX_EarningsCalendars_Ticker",
                table: "EarningsCalendars");
        }
    }
}
