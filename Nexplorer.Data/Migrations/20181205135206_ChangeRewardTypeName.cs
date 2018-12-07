using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class ChangeRewardTypeName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Transaction_RewardType",
                newName: "IX_Transaction_TransactionType",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "RewardType",
                newName: "TransactionType",
                table: "Transaction");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Transaction_TransactionType",
                newName: "IX_Transaction_RewardType",
                table: "Transaction");

            migrationBuilder.RenameColumn(
                name: "TransactionType",
                newName: "RewardType",
                table: "Transaction");
        }
    }
}
