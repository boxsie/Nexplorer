using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Nexplorer.Data.Migrations.NexplorerDbMigrations
{
    public partial class RemoveAccountTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavouriteAddress_Account_AccountId",
                table: "FavouriteAddress");

            migrationBuilder.DropTable(
                name: "Account");

            migrationBuilder.DropIndex(
                name: "IX_FavouriteAddress_AccountId",
                table: "FavouriteAddress");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "FavouriteAddress");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "FavouriteAddress",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Currency",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredOn",
                table: "AspNetUsers",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteAddress_UserId",
                table: "FavouriteAddress",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteAddress_AspNetUsers_UserId",
                table: "FavouriteAddress",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavouriteAddress_AspNetUsers_UserId",
                table: "FavouriteAddress");

            migrationBuilder.DropIndex(
                name: "IX_FavouriteAddress_UserId",
                table: "FavouriteAddress");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "FavouriteAddress");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RegisteredOn",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "FavouriteAddress",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    AccountId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Currency = table.Column<int>(nullable: false),
                    RegisteredOn = table.Column<DateTime>(nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.AccountId);
                    table.ForeignKey(
                        name: "FK_Account_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteAddress_AccountId",
                table: "FavouriteAddress",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Account_UserId",
                table: "Account",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FavouriteAddress_Account_AccountId",
                table: "FavouriteAddress",
                column: "AccountId",
                principalTable: "Account",
                principalColumn: "AccountId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
