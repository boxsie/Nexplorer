using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class SizeDiffBlockIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Block_Difficulty",
                table: "Block",
                column: "Difficulty");

            migrationBuilder.CreateIndex(
                name: "IX_Block_Size",
                table: "Block",
                column: "Size");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Block_Difficulty",
                table: "Block");

            migrationBuilder.DropIndex(
                name: "IX_Block_Size",
                table: "Block");
        }
    }
}
