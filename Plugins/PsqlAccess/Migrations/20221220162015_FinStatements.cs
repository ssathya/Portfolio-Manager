using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class FinStatements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinStatements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Revenue = table.Column<float>(type: "real", nullable: true),
                    SellingGeneralAndAdministrativeExpense = table.Column<float>(type: "real", nullable: true),
                    ResearchAndDevelopmentExpense = table.Column<float>(type: "real", nullable: true),
                    OperatingIncome = table.Column<float>(type: "real", nullable: true),
                    NetIncome = table.Column<float>(type: "real", nullable: true),
                    Assets = table.Column<float>(type: "real", nullable: true),
                    Liabilities = table.Column<float>(type: "real", nullable: true),
                    Equity = table.Column<float>(type: "real", nullable: true),
                    Cash = table.Column<float>(type: "real", nullable: true),
                    CurrentAssets = table.Column<float>(type: "real", nullable: true),
                    CurrentLiabilities = table.Column<float>(type: "real", nullable: true),
                    RetainedEarnings = table.Column<float>(type: "real", nullable: true),
                    CommonStockSharesOutstanding = table.Column<long>(type: "bigint", nullable: true),
                    OperatingCashFlows = table.Column<float>(type: "real", nullable: true),
                    InvestingCashFlows = table.Column<float>(type: "real", nullable: true),
                    FinancingCashFlows = table.Column<float>(type: "real", nullable: true),
                    ProceedsFromIssuanceOfDebt = table.Column<float>(type: "real", nullable: true),
                    PaymentsOfDebt = table.Column<float>(type: "real", nullable: true),
                    DividendsPaid = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinStatements", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinStatements");
        }
    }
}
