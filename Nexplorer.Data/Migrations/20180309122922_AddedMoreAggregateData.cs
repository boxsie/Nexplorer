using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Nexplorer.Data.Migrations
{
    public partial class AddedMoreAggregateData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ReceivedAmount",
                table: "AddressAggregate",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "ReceivedCount",
                table: "AddressAggregate",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SentAmount",
                table: "AddressAggregate",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "SentCount",
                table: "AddressAggregate",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceivedAmount",
                table: "AddressAggregate");

            migrationBuilder.DropColumn(
                name: "ReceivedCount",
                table: "AddressAggregate");

            migrationBuilder.DropColumn(
                name: "SentAmount",
                table: "AddressAggregate");

            migrationBuilder.DropColumn(
                name: "SentCount",
                table: "AddressAggregate");
        }
    }
}
