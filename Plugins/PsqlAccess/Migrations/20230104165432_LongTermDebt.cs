using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class LongTermDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReturnOnEquity",
                table: "ScoreDetails",
                newName: "ZeroDilution");

            migrationBuilder.RenameColumn(
                name: "ChangeInSharesOutstanding",
                table: "ScoreDetails",
                newName: "QualityOfEarnings");

            migrationBuilder.RenameColumn(
                name: "ChangeInROA",
                table: "ScoreDetails",
                newName: "NetIncome");

            migrationBuilder.RenameColumn(
                name: "ChangeInNetIncome",
                table: "ScoreDetails",
                newName: "IncreasedLiquidity");

            migrationBuilder.RenameColumn(
                name: "ChangeInCurrentRatio",
                table: "ScoreDetails",
                newName: "DecreasedLeverage");

            migrationBuilder.RenameColumn(
                name: "ChangInLongTermDebt",
                table: "ScoreDetails",
                newName: "AssetTurnOver");

            migrationBuilder.AddColumn<float>(
                name: "LongTermDebt",
                table: "FinStatements",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LongTermDebt",
                table: "FinStatements");

            migrationBuilder.RenameColumn(
                name: "ZeroDilution",
                table: "ScoreDetails",
                newName: "ReturnOnEquity");

            migrationBuilder.RenameColumn(
                name: "QualityOfEarnings",
                table: "ScoreDetails",
                newName: "ChangeInSharesOutstanding");

            migrationBuilder.RenameColumn(
                name: "NetIncome",
                table: "ScoreDetails",
                newName: "ChangeInROA");

            migrationBuilder.RenameColumn(
                name: "IncreasedLiquidity",
                table: "ScoreDetails",
                newName: "ChangeInNetIncome");

            migrationBuilder.RenameColumn(
                name: "DecreasedLeverage",
                table: "ScoreDetails",
                newName: "ChangeInCurrentRatio");

            migrationBuilder.RenameColumn(
                name: "AssetTurnOver",
                table: "ScoreDetails",
                newName: "ChangInLongTermDebt");
        }
    }
}
