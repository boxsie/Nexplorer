using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class AddBlockIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Block_Channel",
                table: "Block",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_Block_Timestamp",
                table: "Block",
                column: "Timestamp");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Block_Channel",
                table: "Block");

            migrationBuilder.DropIndex(
                name: "IX_Block_Timestamp",
                table: "Block");
        }
    }
}
