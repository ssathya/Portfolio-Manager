using System;
using System.Collections.Generic;
using ApplicationModels.Compute;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsqlAccess.Migrations
{
    /// <inheritdoc />
    public partial class TechnicalAnalysis2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MomentumValue",
                table: "Momentums");

            migrationBuilder.DropColumn(
                name: "ReportingDate",
                table: "Momentums");

            migrationBuilder.AddColumn<List<ComputedValues>>(
                name: "ComputedValues",
                table: "Momentums",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComputedValues",
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
        }
    }
}
