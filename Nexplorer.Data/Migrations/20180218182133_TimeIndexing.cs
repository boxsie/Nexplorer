using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Nexplorer.Data.Migrations
{
    public partial class TimeIndexing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TimeUtc",
                table: "Transaction",
                column: "TimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Block_TimeUtc",
                table: "Block",
                column: "TimeUtc");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transaction_TimeUtc",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Block_TimeUtc",
                table: "Block");
        }
    }
}
