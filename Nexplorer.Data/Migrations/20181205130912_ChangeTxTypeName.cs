using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class ChangeTxTypeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_TransactionInputOutput_TransactionType",
                newName: "IX_TransactionInputOutput_TransactionInputOutputType",
                table: "TransactionInputOutput");

            migrationBuilder.RenameColumn(
                name: "TransactionType", 
                newName: "TransactionInputOutputType", 
                table: "TransactionInputOutput");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_TransactionInputOutput_TransactionInputOutputType",
                newName: "IX_TransactionInputOutput_TransactionType",
                table: "TransactionInputOutput");

            migrationBuilder.RenameColumn(
                name: "TransactionInputOutputType",
                newName: "TransactionType",
                table: "TransactionInputOutput");
        }
    }
}
