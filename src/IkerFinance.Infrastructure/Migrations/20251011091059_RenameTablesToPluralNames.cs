using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkerFinance.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameTablesToPluralNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budget_AspNetUsers_UserId",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_Budget_Currency_CurrencyId",
                table: "Budget");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetCategory_Budget_BudgetId",
                table: "BudgetCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetCategory_Category_CategoryId",
                table: "BudgetCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_Category_AspNetUsers_UserId",
                table: "Category");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRate_AspNetUsers_UpdatedByUserId",
                table: "ExchangeRate");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRate_Currency_FromCurrencyId",
                table: "ExchangeRate");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRate_Currency_ToCurrencyId",
                table: "ExchangeRate");

            migrationBuilder.DropForeignKey(
                name: "FK_Export_AspNetUsers_UserId",
                table: "Export");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_AspNetUsers_RespondedByUserId",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedback_AspNetUsers_UserId",
                table: "Feedback");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_AspNetUsers_UserId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Category_CategoryId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Currency_ConvertedCurrencyId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Currency_CurrencyId",
                table: "Transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Feedback",
                table: "Feedback");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Export",
                table: "Export");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExchangeRate",
                table: "ExchangeRate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currency",
                table: "Currency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetCategory",
                table: "BudgetCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budget",
                table: "Budget");

            migrationBuilder.RenameTable(
                name: "Transaction",
                newName: "Transactions");

            migrationBuilder.RenameTable(
                name: "Feedback",
                newName: "Feedbacks");

            migrationBuilder.RenameTable(
                name: "Export",
                newName: "Exports");

            migrationBuilder.RenameTable(
                name: "ExchangeRate",
                newName: "ExchangeRates");

            migrationBuilder.RenameTable(
                name: "Currency",
                newName: "Currencies");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "BudgetCategory",
                newName: "BudgetCategories");

            migrationBuilder.RenameTable(
                name: "Budget",
                newName: "Budgets");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_UserId_Date",
                table: "Transactions",
                newName: "IX_Transactions_UserId_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_UserId",
                table: "Transactions",
                newName: "IX_Transactions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_Date",
                table: "Transactions",
                newName: "IX_Transactions_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CurrencyId",
                table: "Transactions",
                newName: "IX_Transactions_CurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_ConvertedCurrencyId",
                table: "Transactions",
                newName: "IX_Transactions_ConvertedCurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Transaction_CategoryId",
                table: "Transactions",
                newName: "IX_Transactions_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_UserId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_Status",
                table: "Feedbacks",
                newName: "IX_Feedbacks_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Feedback_RespondedByUserId",
                table: "Feedbacks",
                newName: "IX_Feedbacks_RespondedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Export_UserId",
                table: "Exports",
                newName: "IX_Exports_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Export_ExportDate",
                table: "Exports",
                newName: "IX_Exports_ExportDate");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeRate_UpdatedByUserId",
                table: "ExchangeRates",
                newName: "IX_ExchangeRates_UpdatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeRate_ToCurrencyId",
                table: "ExchangeRates",
                newName: "IX_ExchangeRates_ToCurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeRate_FromCurrencyId_ToCurrencyId_EffectiveDate",
                table: "ExchangeRates",
                newName: "IX_ExchangeRates_FromCurrencyId_ToCurrencyId_EffectiveDate");

            migrationBuilder.RenameIndex(
                name: "IX_Currency_Code",
                table: "Currencies",
                newName: "IX_Currencies_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Category_UserId_Name",
                table: "Categories",
                newName: "IX_Categories_UserId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetCategory_CategoryId",
                table: "BudgetCategories",
                newName: "IX_BudgetCategories_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetCategory_BudgetId_CategoryId",
                table: "BudgetCategories",
                newName: "IX_BudgetCategories_BudgetId_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_UserId_StartDate_EndDate",
                table: "Budgets",
                newName: "IX_Budgets_UserId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_UserId",
                table: "Budgets",
                newName: "IX_Budgets_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Budget_CurrencyId",
                table: "Budgets",
                newName: "IX_Budgets_CurrencyId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feedbacks",
                table: "Feedbacks",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exports",
                table: "Exports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExchangeRates",
                table: "ExchangeRates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetCategories",
                table: "BudgetCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetCategories_Budgets_BudgetId",
                table: "BudgetCategories",
                column: "BudgetId",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetCategories_Categories_CategoryId",
                table: "BudgetCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_AspNetUsers_UserId",
                table: "Budgets",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Currencies_CurrencyId",
                table: "Budgets",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_AspNetUsers_UserId",
                table: "Categories",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRates_AspNetUsers_UpdatedByUserId",
                table: "ExchangeRates",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRates_Currencies_FromCurrencyId",
                table: "ExchangeRates",
                column: "FromCurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRates_Currencies_ToCurrencyId",
                table: "ExchangeRates",
                column: "ToCurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Exports_AspNetUsers_UserId",
                table: "Exports",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_AspNetUsers_RespondedByUserId",
                table: "Feedbacks",
                column: "RespondedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_AspNetUsers_UserId",
                table: "Feedbacks",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Currencies_ConvertedCurrencyId",
                table: "Transactions",
                column: "ConvertedCurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Currencies_CurrencyId",
                table: "Transactions",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BudgetCategories_Budgets_BudgetId",
                table: "BudgetCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetCategories_Categories_CategoryId",
                table: "BudgetCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_AspNetUsers_UserId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Currencies_CurrencyId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_AspNetUsers_UserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRates_AspNetUsers_UpdatedByUserId",
                table: "ExchangeRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRates_Currencies_FromCurrencyId",
                table: "ExchangeRates");

            migrationBuilder.DropForeignKey(
                name: "FK_ExchangeRates_Currencies_ToCurrencyId",
                table: "ExchangeRates");

            migrationBuilder.DropForeignKey(
                name: "FK_Exports_AspNetUsers_UserId",
                table: "Exports");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_AspNetUsers_RespondedByUserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_AspNetUsers_UserId",
                table: "Feedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_AspNetUsers_UserId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Categories_CategoryId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Currencies_ConvertedCurrencyId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Currencies_CurrencyId",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Transactions",
                table: "Transactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Feedbacks",
                table: "Feedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exports",
                table: "Exports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExchangeRates",
                table: "ExchangeRates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Currencies",
                table: "Currencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Budgets",
                table: "Budgets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BudgetCategories",
                table: "BudgetCategories");

            migrationBuilder.RenameTable(
                name: "Transactions",
                newName: "Transaction");

            migrationBuilder.RenameTable(
                name: "Feedbacks",
                newName: "Feedback");

            migrationBuilder.RenameTable(
                name: "Exports",
                newName: "Export");

            migrationBuilder.RenameTable(
                name: "ExchangeRates",
                newName: "ExchangeRate");

            migrationBuilder.RenameTable(
                name: "Currencies",
                newName: "Currency");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameTable(
                name: "Budgets",
                newName: "Budget");

            migrationBuilder.RenameTable(
                name: "BudgetCategories",
                newName: "BudgetCategory");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId_Date",
                table: "Transaction",
                newName: "IX_Transaction_UserId_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_UserId",
                table: "Transaction",
                newName: "IX_Transaction_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_Date",
                table: "Transaction",
                newName: "IX_Transaction_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CurrencyId",
                table: "Transaction",
                newName: "IX_Transaction_CurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ConvertedCurrencyId",
                table: "Transaction",
                newName: "IX_Transaction_ConvertedCurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transaction",
                newName: "IX_Transaction_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_UserId",
                table: "Feedback",
                newName: "IX_Feedback_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_Status",
                table: "Feedback",
                newName: "IX_Feedback_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Feedbacks_RespondedByUserId",
                table: "Feedback",
                newName: "IX_Feedback_RespondedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Exports_UserId",
                table: "Export",
                newName: "IX_Export_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Exports_ExportDate",
                table: "Export",
                newName: "IX_Export_ExportDate");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeRates_UpdatedByUserId",
                table: "ExchangeRate",
                newName: "IX_ExchangeRate_UpdatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeRates_ToCurrencyId",
                table: "ExchangeRate",
                newName: "IX_ExchangeRate_ToCurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_ExchangeRates_FromCurrencyId_ToCurrencyId_EffectiveDate",
                table: "ExchangeRate",
                newName: "IX_ExchangeRate_FromCurrencyId_ToCurrencyId_EffectiveDate");

            migrationBuilder.RenameIndex(
                name: "IX_Currencies_Code",
                table: "Currency",
                newName: "IX_Currency_Code");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_UserId_Name",
                table: "Category",
                newName: "IX_Category_UserId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_UserId_StartDate_EndDate",
                table: "Budget",
                newName: "IX_Budget_UserId_StartDate_EndDate");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_UserId",
                table: "Budget",
                newName: "IX_Budget_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_CurrencyId",
                table: "Budget",
                newName: "IX_Budget_CurrencyId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetCategories_CategoryId",
                table: "BudgetCategory",
                newName: "IX_BudgetCategory_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_BudgetCategories_BudgetId_CategoryId",
                table: "BudgetCategory",
                newName: "IX_BudgetCategory_BudgetId_CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Transaction",
                table: "Transaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Feedback",
                table: "Feedback",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Export",
                table: "Export",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExchangeRate",
                table: "ExchangeRate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Currency",
                table: "Currency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Budget",
                table: "Budget",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BudgetCategory",
                table: "BudgetCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_AspNetUsers_UserId",
                table: "Budget",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Budget_Currency_CurrencyId",
                table: "Budget",
                column: "CurrencyId",
                principalTable: "Currency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetCategory_Budget_BudgetId",
                table: "BudgetCategory",
                column: "BudgetId",
                principalTable: "Budget",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetCategory_Category_CategoryId",
                table: "BudgetCategory",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Category_AspNetUsers_UserId",
                table: "Category",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRate_AspNetUsers_UpdatedByUserId",
                table: "ExchangeRate",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRate_Currency_FromCurrencyId",
                table: "ExchangeRate",
                column: "FromCurrencyId",
                principalTable: "Currency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExchangeRate_Currency_ToCurrencyId",
                table: "ExchangeRate",
                column: "ToCurrencyId",
                principalTable: "Currency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Export_AspNetUsers_UserId",
                table: "Export",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_AspNetUsers_RespondedByUserId",
                table: "Feedback",
                column: "RespondedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Feedback_AspNetUsers_UserId",
                table: "Feedback",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_AspNetUsers_UserId",
                table: "Transaction",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Category_CategoryId",
                table: "Transaction",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Currency_ConvertedCurrencyId",
                table: "Transaction",
                column: "ConvertedCurrencyId",
                principalTable: "Currency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Currency_CurrencyId",
                table: "Transaction",
                column: "CurrencyId",
                principalTable: "Currency",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
