using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IkerFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertToCategoryBasedBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Delete all existing budgets before schema changes
            migrationBuilder.Sql("DELETE FROM \"BudgetCategories\";");
            migrationBuilder.Sql("DELETE FROM \"Budgets\";");

            migrationBuilder.DropTable(
                name: "BudgetCategories");

            migrationBuilder.DropTable(
                name: "Exports");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Budgets");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Budgets",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_CategoryId",
                table: "Budgets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Budgets_UserId_CategoryId_Period_IsActive",
                table: "Budgets",
                columns: new[] { "UserId", "CategoryId", "Period", "IsActive" },
                filter: "\"IsActive\" = true");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Categories_CategoryId",
                table: "Budgets",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Categories_CategoryId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_CategoryId",
                table: "Budgets");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_UserId_CategoryId_Period_IsActive",
                table: "Budgets");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Budgets");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Budgets",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BudgetCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BudgetId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetCategories_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BudgetCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Exports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Content = table.Column<int>(type: "integer", nullable: false),
                    DateFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DateTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExportDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Filters = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IncludeBudgetSummary = table.Column<bool>(type: "boolean", nullable: false),
                    RecordCount = table.Column<int>(type: "integer", nullable: false),
                    SelectedCategories = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exports_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AdminResponse = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    RespondedByUserId = table.Column<string>(type: "text", nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Feedbacks_AspNetUsers_RespondedByUserId",
                        column: x => x.RespondedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Feedbacks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCategories_BudgetId_CategoryId",
                table: "BudgetCategories",
                columns: new[] { "BudgetId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetCategories_CategoryId",
                table: "BudgetCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Exports_ExportDate",
                table: "Exports",
                column: "ExportDate");

            migrationBuilder.CreateIndex(
                name: "IX_Exports_UserId",
                table: "Exports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_RespondedByUserId",
                table: "Feedbacks",
                column: "RespondedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_Status",
                table: "Feedbacks",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedbacks",
                column: "UserId");
        }
    }
}
