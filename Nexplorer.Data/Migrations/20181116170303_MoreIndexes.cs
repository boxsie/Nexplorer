using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class MoreIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TransactionInputOutput_Amount",
                table: "TransactionInputOutput",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInputOutput_TransactionType",
                table: "TransactionInputOutput",
                column: "TransactionType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionInputOutput_Amount",
                table: "TransactionInputOutput");

            migrationBuilder.DropIndex(
                name: "IX_TransactionInputOutput_TransactionType",
                table: "TransactionInputOutput");
        }
    }
}
