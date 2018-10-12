using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Nexplorer.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BittrexSummary",
                columns: table => new
                {
                    BittrexSummaryId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Ask = table.Column<double>(nullable: false),
                    BaseVolume = table.Column<double>(nullable: false),
                    Bid = table.Column<double>(nullable: false),
                    Last = table.Column<double>(nullable: false),
                    MarketName = table.Column<string>(maxLength: 10, nullable: false),
                    OpenBuyOrders = table.Column<int>(nullable: false),
                    OpenSellOrders = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false),
                    Volume = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BittrexSummary", x => x.BittrexSummaryId);
                });

            migrationBuilder.CreateTable(
                name: "Block",
                columns: table => new
                {
                    Height = table.Column<int>(nullable: false),
                    Bits = table.Column<string>(nullable: true),
                    Channel = table.Column<int>(nullable: false),
                    Difficulty = table.Column<double>(nullable: false),
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    MerkleRoot = table.Column<string>(nullable: true),
                    Mint = table.Column<double>(nullable: false),
                    Nonce = table.Column<double>(nullable: false),
                    Size = table.Column<int>(nullable: false),
                    TimeUtc = table.Column<DateTime>(nullable: false),
                    Version = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Block", x => x.Height);
                });

            migrationBuilder.CreateTable(
                name: "OrphanBlock",
                columns: table => new
                {
                    BlockId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    Height = table.Column<int>(nullable: false),
                    TimeUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrphanBlock", x => x.BlockId);
                });

            migrationBuilder.CreateTable(
                name: "Address",
                columns: table => new
                {
                    AddressId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FirstBlockHeight = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Address", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_Address_Block_FirstBlockHeight",
                        column: x => x.FirstBlockHeight,
                        principalTable: "Block",
                        principalColumn: "Height",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<double>(nullable: false),
                    BlockHeight = table.Column<int>(nullable: false),
                    Confirmations = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    TimeUtc = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transaction_Block_BlockHeight",
                        column: x => x.BlockHeight,
                        principalTable: "Block",
                        principalColumn: "Height",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrphanTransaction",
                columns: table => new
                {
                    TransactionId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BlockHeight = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    OrphanBlockBlockId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrphanTransaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_OrphanTransaction_OrphanBlock_OrphanBlockBlockId",
                        column: x => x.OrphanBlockBlockId,
                        principalTable: "OrphanBlock",
                        principalColumn: "BlockId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionInput",
                columns: table => new
                {
                    TransactionInputId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AddressId = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionInput", x => x.TransactionInputId);
                    table.ForeignKey(
                        name: "FK_TransactionInput_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionInput_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionOutput",
                columns: table => new
                {
                    TransactionOutputId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AddressId = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionOutput", x => x.TransactionOutputId);
                    table.ForeignKey(
                        name: "FK_TransactionOutput_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionOutput_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Address_FirstBlockHeight",
                table: "Address",
                column: "FirstBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Hash",
                table: "Address",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Block_Hash",
                table: "Block",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanBlock_Hash",
                table: "OrphanBlock",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanTransaction_Hash",
                table: "OrphanTransaction",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanTransaction_OrphanBlockBlockId",
                table: "OrphanTransaction",
                column: "OrphanBlockBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_BlockHeight",
                table: "Transaction",
                column: "BlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Hash",
                table: "Transaction",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInput_AddressId",
                table: "TransactionInput",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInput_Amount",
                table: "TransactionInput",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInput_TransactionId",
                table: "TransactionInput",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutput_AddressId",
                table: "TransactionOutput",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutput_Amount",
                table: "TransactionOutput",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionOutput_TransactionId",
                table: "TransactionOutput",
                column: "TransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BittrexSummary");

            migrationBuilder.DropTable(
                name: "OrphanTransaction");

            migrationBuilder.DropTable(
                name: "TransactionInput");

            migrationBuilder.DropTable(
                name: "TransactionOutput");

            migrationBuilder.DropTable(
                name: "OrphanBlock");

            migrationBuilder.DropTable(
                name: "Address");

            migrationBuilder.DropTable(
                name: "Transaction");

            migrationBuilder.DropTable(
                name: "Block");
        }
    }
}
