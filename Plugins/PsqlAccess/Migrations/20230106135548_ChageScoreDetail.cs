using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChageScoreDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ZeroDilution",
                table: "ScoreDetails",
                newName: "IsROABetter");

            migrationBuilder.RenameColumn(
                name: "QualityOfEarnings",
                table: "ScoreDetails",
                newName: "IncreaseGrossMargin");

            migrationBuilder.RenameColumn(
                name: "NetIncome",
                table: "ScoreDetails",
                newName: "ChangeInNumberOfShares");

            migrationBuilder.RenameColumn(
                name: "IncreasedLiquidity",
                table: "ScoreDetails",
                newName: "ChangeInLeverage");

            migrationBuilder.RenameColumn(
                name: "GrossMargin",
                table: "ScoreDetails",
                newName: "ChangeInCurrentRatio");

            migrationBuilder.RenameColumn(
                name: "DecreasedLeverage",
                table: "ScoreDetails",
                newName: "AssetTurnoverRatio");

            migrationBuilder.RenameColumn(
                name: "AssetTurnOver",
                table: "ScoreDetails",
                newName: "Accruals");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsROABetter",
                table: "ScoreDetails",
                newName: "ZeroDilution");

            migrationBuilder.RenameColumn(
                name: "IncreaseGrossMargin",
                table: "ScoreDetails",
                newName: "QualityOfEarnings");

            migrationBuilder.RenameColumn(
                name: "ChangeInNumberOfShares",
                table: "ScoreDetails",
                newName: "NetIncome");

            migrationBuilder.RenameColumn(
                name: "ChangeInLeverage",
                table: "ScoreDetails",
                newName: "IncreasedLiquidity");

            migrationBuilder.RenameColumn(
                name: "ChangeInCurrentRatio",
                table: "ScoreDetails",
                newName: "GrossMargin");

            migrationBuilder.RenameColumn(
                name: "AssetTurnoverRatio",
                table: "ScoreDetails",
                newName: "DecreasedLeverage");

            migrationBuilder.RenameColumn(
                name: "Accruals",
                table: "ScoreDetails",
                newName: "AssetTurnOver");
        }
    }
}
