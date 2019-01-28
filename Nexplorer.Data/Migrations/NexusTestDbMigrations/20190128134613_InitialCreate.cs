using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations.NexusTestDbMigrations
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MarketName = table.Column<string>(maxLength: 10, nullable: false),
                    Volume = table.Column<double>(nullable: false),
                    BaseVolume = table.Column<double>(nullable: false),
                    Last = table.Column<double>(nullable: false),
                    Bid = table.Column<double>(nullable: false),
                    Ask = table.Column<double>(nullable: false),
                    OpenBuyOrders = table.Column<int>(nullable: false),
                    OpenSellOrders = table.Column<int>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
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
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    Size = table.Column<int>(nullable: false),
                    Channel = table.Column<int>(nullable: false),
                    Version = table.Column<int>(nullable: false),
                    MerkleRoot = table.Column<string>(maxLength: 256, nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Nonce = table.Column<double>(nullable: false),
                    Bits = table.Column<string>(maxLength: 256, nullable: false),
                    Difficulty = table.Column<double>(nullable: false),
                    Mint = table.Column<double>(nullable: false)
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Height = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false)
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    FirstBlockHeight = table.Column<int>(nullable: false)
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlockHeight = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(maxLength: 256, nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    TransactionType = table.Column<int>(nullable: true)
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
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    BlockHeight = table.Column<int>(nullable: false),
                    Hash = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrphanTransaction", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_OrphanTransaction_OrphanBlock_BlockHeight",
                        column: x => x.BlockHeight,
                        principalTable: "OrphanBlock",
                        principalColumn: "BlockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AddressAggregate",
                columns: table => new
                {
                    AddressId = table.Column<int>(nullable: false),
                    LastBlockHeight = table.Column<int>(nullable: false),
                    Balance = table.Column<double>(nullable: false),
                    ReceivedAmount = table.Column<double>(nullable: false),
                    ReceivedCount = table.Column<int>(nullable: false),
                    SentAmount = table.Column<double>(nullable: false),
                    SentCount = table.Column<int>(nullable: false),
                    UpdatedOn = table.Column<DateTime>(nullable: false)
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
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TransactionInputOutput",
                columns: table => new
                {
                    TransactionInputOutputId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TransactionId = table.Column<int>(nullable: false),
                    TransactionInputOutputType = table.Column<int>(nullable: false),
                    AddressId = table.Column<int>(nullable: false),
                    Amount = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionInputOutput", x => x.TransactionInputOutputId);
                    table.ForeignKey(
                        name: "FK_TransactionInputOutput_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionInputOutput_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrustKey",
                columns: table => new
                {
                    TrustKeyId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    GenesisHeight = table.Column<int>(nullable: false),
                    AddressId = table.Column<int>(nullable: false),
                    TransactionId = table.Column<int>(nullable: false),
                    Key = table.Column<string>(nullable: false),
                    Hash = table.Column<string>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustKey", x => x.TrustKeyId);
                    table.ForeignKey(
                        name: "FK_TrustKey_Address_AddressId",
                        column: x => x.AddressId,
                        principalTable: "Address",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrustKey_Block_GenesisHeight",
                        column: x => x.GenesisHeight,
                        principalTable: "Block",
                        principalColumn: "Height",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrustKey_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Restrict);
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
                name: "IX_AddressAggregate_Balance",
                table: "AddressAggregate",
                column: "Balance");

            migrationBuilder.CreateIndex(
                name: "IX_AddressAggregate_LastBlockHeight",
                table: "AddressAggregate",
                column: "LastBlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_Block_Channel",
                table: "Block",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_Block_Hash",
                table: "Block",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Block_Timestamp",
                table: "Block",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_OrphanTransaction_BlockHeight",
                table: "OrphanTransaction",
                column: "BlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Amount",
                table: "Transaction",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_BlockHeight",
                table: "Transaction",
                column: "BlockHeight");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Hash",
                table: "Transaction",
                column: "Hash");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_Timestamp",
                table: "Transaction",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionType",
                table: "Transaction",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInputOutput_AddressId",
                table: "TransactionInputOutput",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInputOutput_Amount",
                table: "TransactionInputOutput",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInputOutput_TransactionId",
                table: "TransactionInputOutput",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInputOutput_TransactionInputOutputType",
                table: "TransactionInputOutput",
                column: "TransactionInputOutputType");

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_AddressId",
                table: "TrustKey",
                column: "AddressId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_GenesisHeight",
                table: "TrustKey",
                column: "GenesisHeight");

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_TransactionId",
                table: "TrustKey",
                column: "TransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressAggregate");

            migrationBuilder.DropTable(
                name: "BittrexSummary");

            migrationBuilder.DropTable(
                name: "OrphanTransaction");

            migrationBuilder.DropTable(
                name: "TransactionInputOutput");

            migrationBuilder.DropTable(
                name: "TrustKey");

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
