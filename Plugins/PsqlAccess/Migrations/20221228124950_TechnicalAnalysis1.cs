using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class TechnicalAnalysis1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MomentumValues",
                table: "Momentums");

            migrationBuilder.AddColumn<decimal>(
                name: "MomentumValue",
                table: "Momentums",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportingDate",
                table: "Momentums",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Momentums_Ticker",
                table: "Momentums",
                column: "Ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Momentums_Ticker",
                table: "Momentums");

            migrationBuilder.DropColumn(
                name: "MomentumValue",
                table: "Momentums");

            migrationBuilder.DropColumn(
                name: "ReportingDate",
                table: "Momentums");

            migrationBuilder.AddColumn<List<decimal>>(
                name: "MomentumValues",
                table: "Momentums",
                type: "numeric[]",
                nullable: false);
        }
    }
}
