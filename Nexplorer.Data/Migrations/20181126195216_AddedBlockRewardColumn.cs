using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class AddedBlockRewardColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RewardType",
                table: "Transaction",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_RewardType",
                table: "Transaction",
                column: "RewardType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_RewardType",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "RewardType",
                table: "Transaction");
        }
    }
}
