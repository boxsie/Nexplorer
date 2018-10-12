using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Nexplorer.Data.Migrations
{
    public partial class AddUniqueIndexAddressHash : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Address_Hash",
                table: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Hash",
                table: "Address",
                column: "Hash",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Address_Hash",
                table: "Address");

            migrationBuilder.CreateIndex(
                name: "IX_Address_Hash",
                table: "Address",
                column: "Hash");
        }
    }
}
