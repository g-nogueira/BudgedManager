using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyBudget.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNavigationMappings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_daily_entries_forecast_versions_ForecastVersionForecastId",
                schema: "forecast",
                table: "daily_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_daily_expense_items_daily_entries_DailyEntryEntryId",
                schema: "forecast",
                table: "daily_expense_items");

            migrationBuilder.DropForeignKey(
                name: "FK_expense_snapshots_forecast_versions_ForecastVersionForecast~",
                schema: "forecast",
                table: "expense_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_expense_snapshots_ForecastVersionForecastId",
                schema: "forecast",
                table: "expense_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_daily_expense_items_DailyEntryEntryId",
                schema: "forecast",
                table: "daily_expense_items");

            migrationBuilder.DropIndex(
                name: "IX_daily_entries_ForecastVersionForecastId",
                schema: "forecast",
                table: "daily_entries");

            migrationBuilder.DropColumn(
                name: "ForecastVersionForecastId",
                schema: "forecast",
                table: "expense_snapshots");

            migrationBuilder.DropColumn(
                name: "DailyEntryEntryId",
                schema: "forecast",
                table: "daily_expense_items");

            migrationBuilder.DropColumn(
                name: "ForecastVersionForecastId",
                schema: "forecast",
                table: "daily_entries");

            migrationBuilder.CreateIndex(
                name: "IX_expense_snapshots_forecast_id",
                schema: "forecast",
                table: "expense_snapshots",
                column: "forecast_id");

            migrationBuilder.CreateIndex(
                name: "IX_daily_expense_items_entry_id",
                schema: "forecast",
                table: "daily_expense_items",
                column: "entry_id");

            migrationBuilder.AddForeignKey(
                name: "FK_daily_entries_forecast_versions_forecast_id",
                schema: "forecast",
                table: "daily_entries",
                column: "forecast_id",
                principalSchema: "forecast",
                principalTable: "forecast_versions",
                principalColumn: "forecast_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_daily_expense_items_daily_entries_entry_id",
                schema: "forecast",
                table: "daily_expense_items",
                column: "entry_id",
                principalSchema: "forecast",
                principalTable: "daily_entries",
                principalColumn: "entry_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_expense_snapshots_forecast_versions_forecast_id",
                schema: "forecast",
                table: "expense_snapshots",
                column: "forecast_id",
                principalSchema: "forecast",
                principalTable: "forecast_versions",
                principalColumn: "forecast_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_daily_entries_forecast_versions_forecast_id",
                schema: "forecast",
                table: "daily_entries");

            migrationBuilder.DropForeignKey(
                name: "FK_daily_expense_items_daily_entries_entry_id",
                schema: "forecast",
                table: "daily_expense_items");

            migrationBuilder.DropForeignKey(
                name: "FK_expense_snapshots_forecast_versions_forecast_id",
                schema: "forecast",
                table: "expense_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_expense_snapshots_forecast_id",
                schema: "forecast",
                table: "expense_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_daily_expense_items_entry_id",
                schema: "forecast",
                table: "daily_expense_items");

            migrationBuilder.AddColumn<Guid>(
                name: "ForecastVersionForecastId",
                schema: "forecast",
                table: "expense_snapshots",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DailyEntryEntryId",
                schema: "forecast",
                table: "daily_expense_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ForecastVersionForecastId",
                schema: "forecast",
                table: "daily_entries",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_expense_snapshots_ForecastVersionForecastId",
                schema: "forecast",
                table: "expense_snapshots",
                column: "ForecastVersionForecastId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_expense_items_DailyEntryEntryId",
                schema: "forecast",
                table: "daily_expense_items",
                column: "DailyEntryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_entries_ForecastVersionForecastId",
                schema: "forecast",
                table: "daily_entries",
                column: "ForecastVersionForecastId");

            migrationBuilder.AddForeignKey(
                name: "FK_daily_entries_forecast_versions_ForecastVersionForecastId",
                schema: "forecast",
                table: "daily_entries",
                column: "ForecastVersionForecastId",
                principalSchema: "forecast",
                principalTable: "forecast_versions",
                principalColumn: "forecast_id");

            migrationBuilder.AddForeignKey(
                name: "FK_daily_expense_items_daily_entries_DailyEntryEntryId",
                schema: "forecast",
                table: "daily_expense_items",
                column: "DailyEntryEntryId",
                principalSchema: "forecast",
                principalTable: "daily_entries",
                principalColumn: "entry_id");

            migrationBuilder.AddForeignKey(
                name: "FK_expense_snapshots_forecast_versions_ForecastVersionForecast~",
                schema: "forecast",
                table: "expense_snapshots",
                column: "ForecastVersionForecastId",
                principalSchema: "forecast",
                principalTable: "forecast_versions",
                principalColumn: "forecast_id");
        }
    }
}
