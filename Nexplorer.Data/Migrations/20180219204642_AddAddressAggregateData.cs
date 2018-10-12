using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Nexplorer.Data.Migrations
{
    public partial class AddAddressAggregateData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressAggregate",
                columns: table => new
                {
                    AddressId = table.Column<int>(nullable: false),
                    Balance = table.Column<double>(nullable: false),
                    LastBlockHeight = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressAggregate", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_AddressAggregate_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddressAggregate_Block_LastBlockHeight",
                        column: x => x.LastBlockHeight,
                        principalTable: "Block",
                        principalColumn: "Height",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressAggregate_Balance",
                table: "AddressAggregate",
                column: "Balance");

            migrationBuilder.CreateIndex(
                name: "IX_AddressAggregate_LastBlockHeight",
                table: "AddressAggregate",
                column: "LastBlockHeight");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressAggregate");
        }
    }
}
