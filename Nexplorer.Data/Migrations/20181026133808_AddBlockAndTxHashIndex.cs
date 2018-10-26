using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class AddBlockAndTxHashIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Hash",
                table: "Transaction",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Block_Hash",
                table: "Block",
                column: "Hash");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_Hash",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Block_Hash",
                table: "Block");
        }
    }
}
