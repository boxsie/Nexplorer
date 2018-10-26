using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class AddAggregateBalanceIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_AddressAggregate_Balance",
                table: "AddressAggregate",
                column: "Balance");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AddressAggregate_Balance",
                table: "AddressAggregate");
        }
    }
}
