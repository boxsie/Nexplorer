using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Nexplorer.Data.Migrations
{
    public partial class AddTrustKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrustKey",
                columns: table => new
                {
                    TrustKeyId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AddressId = table.Column<int>(nullable: false),
                    GenesisBlockHeight = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustKey", x => x.TrustKeyId);
                    table.ForeignKey(
                        name: "FK_TrustKey_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrustKey_Block_GenesisBlockHeight",
                        column: x => x.GenesisBlockHeight,
                        principalTable: "Block",
                        principalColumn: "Height",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrustKey_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_AddressId",
                table: "TrustKey",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_GenesisBlockHeight",
                table: "TrustKey",
                column: "GenesisBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_TransactionId",
                table: "TrustKey",
                column: "TransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrustKey");
        }
    }
}
