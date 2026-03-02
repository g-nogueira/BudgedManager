using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MonthlyBudget.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "identity");

            migrationBuilder.EnsureSchema(
                name: "forecast");

            migrationBuilder.EnsureSchema(
                name: "budget");

            migrationBuilder.CreateTable(
                name: "app_users",
                schema: "identity",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_users", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "forecast_versions",
                schema: "forecast",
                columns: table => new
                {
                    forecast_id = table.Column<Guid>(type: "uuid", nullable: false),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    forecast_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_day = table.Column<int>(type: "integer", nullable: false),
                    start_balance = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    actual_balance = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    forecast_type = table.Column<string>(type: "text", nullable: false),
                    parent_forecast_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_snapshot = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_forecast_versions", x => x.forecast_id);
                });

            migrationBuilder.CreateTable(
                name: "households",
                schema: "identity",
                columns: table => new
                {
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_households", x => x.household_id);
                });

            migrationBuilder.CreateTable(
                name: "invitations",
                schema: "identity",
                columns: table => new
                {
                    invitation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.invitation_id);
                });

            migrationBuilder.CreateTable(
                name: "monthly_budgets",
                schema: "budget",
                columns: table => new
                {
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    household_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year_month = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_monthly_budgets", x => x.budget_id);
                });

            migrationBuilder.CreateTable(
                name: "daily_entries",
                schema: "forecast",
                columns: table => new
                {
                    entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forecast_id = table.Column<Guid>(type: "uuid", nullable: false),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    remaining_balance = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    daily_expense_total = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ForecastVersionForecastId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_entries", x => x.entry_id);
                    table.ForeignKey(
                        name: "FK_daily_entries_forecast_versions_ForecastVersionForecastId",
                        column: x => x.ForecastVersionForecastId,
                        principalSchema: "forecast",
                        principalTable: "forecast_versions",
                        principalColumn: "forecast_id");
                });

            migrationBuilder.CreateTable(
                name: "expense_snapshots",
                schema: "forecast",
                columns: table => new
                {
                    snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    forecast_id = table.Column<Guid>(type: "uuid", nullable: false),
                    original_expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    day_of_month = table.Column<int>(type: "integer", nullable: true),
                    is_spread = table.Column<bool>(type: "boolean", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    is_excluded = table.Column<bool>(type: "boolean", nullable: false),
                    ForecastVersionForecastId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_snapshots", x => x.snapshot_id);
                    table.ForeignKey(
                        name: "FK_expense_snapshots_forecast_versions_ForecastVersionForecast~",
                        column: x => x.ForecastVersionForecastId,
                        principalSchema: "forecast",
                        principalTable: "forecast_versions",
                        principalColumn: "forecast_id");
                });

            migrationBuilder.CreateTable(
                name: "household_members",
                schema: "identity",
                columns: table => new
                {
                    member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    joined_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HouseholdId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_household_members", x => x.member_id);
                    table.ForeignKey(
                        name: "FK_household_members_households_HouseholdId",
                        column: x => x.HouseholdId,
                        principalSchema: "identity",
                        principalTable: "households",
                        principalColumn: "household_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                schema: "budget",
                columns: table => new
                {
                    expense_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    day_of_month = table.Column<int>(type: "integer", nullable: true),
                    is_spread = table.Column<bool>(type: "boolean", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    is_excluded = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthlyBudgetBudgetId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.expense_id);
                    table.ForeignKey(
                        name: "FK_expenses_monthly_budgets_MonthlyBudgetBudgetId",
                        column: x => x.MonthlyBudgetBudgetId,
                        principalSchema: "budget",
                        principalTable: "monthly_budgets",
                        principalColumn: "budget_id");
                    table.ForeignKey(
                        name: "FK_expenses_monthly_budgets_budget_id",
                        column: x => x.budget_id,
                        principalSchema: "budget",
                        principalTable: "monthly_budgets",
                        principalColumn: "budget_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "income_sources",
                schema: "budget",
                columns: table => new
                {
                    income_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    budget_id = table.Column<Guid>(type: "uuid", nullable: false),
                    MonthlyBudgetBudgetId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_income_sources", x => x.income_id);
                    table.ForeignKey(
                        name: "FK_income_sources_monthly_budgets_MonthlyBudgetBudgetId",
                        column: x => x.MonthlyBudgetBudgetId,
                        principalSchema: "budget",
                        principalTable: "monthly_budgets",
                        principalColumn: "budget_id");
                    table.ForeignKey(
                        name: "FK_income_sources_monthly_budgets_budget_id",
                        column: x => x.budget_id,
                        principalSchema: "budget",
                        principalTable: "monthly_budgets",
                        principalColumn: "budget_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "daily_expense_items",
                schema: "forecast",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entry_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expense_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    DailyEntryEntryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_daily_expense_items", x => x.item_id);
                    table.ForeignKey(
                        name: "FK_daily_expense_items_daily_entries_DailyEntryEntryId",
                        column: x => x.DailyEntryEntryId,
                        principalSchema: "forecast",
                        principalTable: "daily_entries",
                        principalColumn: "entry_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_users_email",
                schema: "identity",
                table: "app_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_entries_forecast_id_day_number",
                schema: "forecast",
                table: "daily_entries",
                columns: new[] { "forecast_id", "day_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_daily_entries_ForecastVersionForecastId",
                schema: "forecast",
                table: "daily_entries",
                column: "ForecastVersionForecastId");

            migrationBuilder.CreateIndex(
                name: "IX_daily_expense_items_DailyEntryEntryId",
                schema: "forecast",
                table: "daily_expense_items",
                column: "DailyEntryEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_expense_snapshots_ForecastVersionForecastId",
                schema: "forecast",
                table: "expense_snapshots",
                column: "ForecastVersionForecastId");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_budget_id",
                schema: "budget",
                table: "expenses",
                column: "budget_id");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_MonthlyBudgetBudgetId",
                schema: "budget",
                table: "expenses",
                column: "MonthlyBudgetBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_household_members_HouseholdId",
                schema: "identity",
                table: "household_members",
                column: "HouseholdId");

            migrationBuilder.CreateIndex(
                name: "IX_income_sources_budget_id",
                schema: "budget",
                table: "income_sources",
                column: "budget_id");

            migrationBuilder.CreateIndex(
                name: "IX_income_sources_MonthlyBudgetBudgetId",
                schema: "budget",
                table: "income_sources",
                column: "MonthlyBudgetBudgetId");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_token",
                schema: "identity",
                table: "invitations",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_monthly_budgets_household_id_year_month",
                schema: "budget",
                table: "monthly_budgets",
                columns: new[] { "household_id", "year_month" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_users",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "daily_expense_items",
                schema: "forecast");

            migrationBuilder.DropTable(
                name: "expense_snapshots",
                schema: "forecast");

            migrationBuilder.DropTable(
                name: "expenses",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "household_members",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "income_sources",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "invitations",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "daily_entries",
                schema: "forecast");

            migrationBuilder.DropTable(
                name: "households",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "monthly_budgets",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "forecast_versions",
                schema: "forecast");
        }
    }
}
