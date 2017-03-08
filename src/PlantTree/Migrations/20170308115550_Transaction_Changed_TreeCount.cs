using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlantTree.Migrations
{
    public partial class Transaction_Changed_TreeCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TreePlantedCount",
                table: "Transactions",
                newName: "TreeCount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TreeCount",
                table: "Transactions",
                newName: "TreePlantedCount");
        }
    }
}
