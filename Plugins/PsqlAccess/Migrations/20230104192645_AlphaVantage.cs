using System;
using System.Collections.Generic;
using ApplicationModels.FinancialStatement.AlphaVantage;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class AlphaVantage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BalanceSheets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    AnnualReports = table.Column<List<BSReport>>(type: "jsonb", nullable: false),
                    QuarterlyReports = table.Column<List<BSReport>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceSheets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashFlows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    AnnualReports = table.Column<List<CashFlowReport>>(type: "jsonb", nullable: false),
                    QuarterlyReports = table.Column<List<CashFlowReport>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncomeStatements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    AnnualReports = table.Column<List<IncomeReport>>(type: "jsonb", nullable: false),
                    QuarterlyReports = table.Column<List<IncomeReport>>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeStatements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Overviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    Ticker = table.Column<string>(type: "text", nullable: false),
                    AssetType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CIK = table.Column<string>(type: "text", nullable: false),
                    Exchange = table.Column<string>(type: "text", nullable: false),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    Country = table.Column<string>(type: "text", nullable: false),
                    Sector = table.Column<string>(type: "text", nullable: false),
                    Industry = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: false),
                    FiscalYearEnd = table.Column<string>(type: "text", nullable: false),
                    LatestQuarter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MarketCapitalization = table.Column<decimal>(type: "numeric", nullable: true),
                    EBITDA = table.Column<decimal>(type: "numeric", nullable: true),
                    PERatio = table.Column<decimal>(type: "numeric", nullable: true),
                    PEGRatio = table.Column<decimal>(type: "numeric", nullable: true),
                    BookValue = table.Column<decimal>(type: "numeric", nullable: true),
                    DividendPerShare = table.Column<decimal>(type: "numeric", nullable: true),
                    DividendYield = table.Column<decimal>(type: "numeric", nullable: true),
                    EPS = table.Column<decimal>(type: "numeric", nullable: true),
                    RevenuePerShareTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    ProfitMargin = table.Column<decimal>(type: "numeric", nullable: true),
                    OperatingMarginTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    ReturnOnAssetsTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    ReturnOnEquityTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    RevenueTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    GrossProfitTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    DilutedEPSTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    QuarterlyEarningsGrowthYOY = table.Column<decimal>(type: "numeric", nullable: true),
                    QuarterlyRevenueGrowthYOY = table.Column<decimal>(type: "numeric", nullable: true),
                    AnalystTargetPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    TrailingPE = table.Column<decimal>(type: "numeric", nullable: true),
                    ForwardPE = table.Column<decimal>(type: "numeric", nullable: true),
                    PriceToSalesRatioTTM = table.Column<decimal>(type: "numeric", nullable: true),
                    PriceToBookRatio = table.Column<decimal>(type: "numeric", nullable: true),
                    EVToRevenue = table.Column<decimal>(type: "numeric", nullable: true),
                    EVToEBITDA = table.Column<decimal>(type: "numeric", nullable: true),
                    Beta = table.Column<decimal>(type: "numeric", nullable: true),
                    _52WeekHigh = table.Column<decimal>(type: "numeric", nullable: true),
                    _52WeekLow = table.Column<decimal>(type: "numeric", nullable: true),
                    _50DayMovingAverage = table.Column<decimal>(type: "numeric", nullable: true),
                    _200DayMovingAverage = table.Column<decimal>(type: "numeric", nullable: true),
                    SharesOutstanding = table.Column<decimal>(type: "numeric", nullable: true),
                    SharesFloat = table.Column<decimal>(type: "numeric", nullable: true),
                    SharesShort = table.Column<decimal>(type: "numeric", nullable: true),
                    SharesShortPriorMonth = table.Column<decimal>(type: "numeric", nullable: true),
                    ShortRatio = table.Column<decimal>(type: "numeric", nullable: true),
                    ShortPercentOutstanding = table.Column<decimal>(type: "numeric", nullable: true),
                    ShortPercentFloat = table.Column<decimal>(type: "numeric", nullable: true),
                    PercentInsiders = table.Column<decimal>(type: "numeric", nullable: true),
                    PercentInstitutions = table.Column<decimal>(type: "numeric", nullable: true),
                    ForwardAnnualDividendRate = table.Column<decimal>(type: "numeric", nullable: true),
                    ForwardAnnualDividendYield = table.Column<decimal>(type: "numeric", nullable: true),
                    PayoutRatio = table.Column<decimal>(type: "numeric", nullable: true),
                    DividendDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExDividendDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSplitFactor = table.Column<string>(type: "text", nullable: false),
                    LastSplitDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Overviews", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalanceSheets_Ticker",
                table: "BalanceSheets",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_CashFlows_Ticker",
                table: "CashFlows",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeStatements_Ticker",
                table: "IncomeStatements",
                column: "Ticker");

            migrationBuilder.CreateIndex(
                name: "IX_Overviews_Ticker",
                table: "Overviews",
                column: "Ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceSheets");

            migrationBuilder.DropTable(
                name: "CashFlows");

            migrationBuilder.DropTable(
                name: "IncomeStatements");

            migrationBuilder.DropTable(
                name: "Overviews");
        }
    }
}
