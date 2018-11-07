using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations
{
    public partial class RemovedAllFKs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressAggregate_Block_LastBlockHeight",
                table: "AddressAggregate");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionInputOutput_Transaction_TransactionId",
                table: "TransactionInputOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_TrustKey_Address_AddressId",
                table: "TrustKey");

            migrationBuilder.DropForeignKey(
                name: "FK_TrustKey_Block_GenesisHeight",
                table: "TrustKey");

            migrationBuilder.DropForeignKey(
                name: "FK_TrustKey_Transaction_TransactionId",
                table: "TrustKey");

            migrationBuilder.DropIndex(
                name: "IX_TrustKey_AddressId",
                table: "TrustKey");

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_AddressId",
                table: "TrustKey",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressAggregate_Block_LastBlockHeight",
                table: "AddressAggregate",
                column: "LastBlockHeight",
                principalTable: "Block",
                principalColumn: "Height",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionInputOutput_Transaction_TransactionId",
                table: "TransactionInputOutput",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrustKey_Address_AddressId",
                table: "TrustKey",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrustKey_Block_GenesisHeight",
                table: "TrustKey",
                column: "GenesisHeight",
                principalTable: "Block",
                principalColumn: "Height",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrustKey_Transaction_TransactionId",
                table: "TrustKey",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressAggregate_Block_LastBlockHeight",
                table: "AddressAggregate");

            migrationBuilder.DropForeignKey(
                name: "FK_TransactionInputOutput_Transaction_TransactionId",
                table: "TransactionInputOutput");

            migrationBuilder.DropForeignKey(
                name: "FK_TrustKey_Address_AddressId",
                table: "TrustKey");

            migrationBuilder.DropForeignKey(
                name: "FK_TrustKey_Block_GenesisHeight",
                table: "TrustKey");

            migrationBuilder.DropForeignKey(
                name: "FK_TrustKey_Transaction_TransactionId",
                table: "TrustKey");

            migrationBuilder.DropIndex(
                name: "IX_TrustKey_AddressId",
                table: "TrustKey");

            migrationBuilder.CreateIndex(
                name: "IX_TrustKey_AddressId",
                table: "TrustKey",
                column: "AddressId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AddressAggregate_Block_LastBlockHeight",
                table: "AddressAggregate",
                column: "LastBlockHeight",
                principalTable: "Block",
                principalColumn: "Height",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionInputOutput_Transaction_TransactionId",
                table: "TransactionInputOutput",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrustKey_Address_AddressId",
                table: "TrustKey",
                column: "AddressId",
                principalTable: "Address",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrustKey_Block_GenesisHeight",
                table: "TrustKey",
                column: "GenesisHeight",
                principalTable: "Block",
                principalColumn: "Height",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrustKey_Transaction_TransactionId",
                table: "TrustKey",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
