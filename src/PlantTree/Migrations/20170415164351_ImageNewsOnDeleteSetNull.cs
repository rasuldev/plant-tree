using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PlantTree.Migrations
{
    public partial class ImageNewsOnDeleteSetNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_Images_PhotoId",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "News",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShortText",
                table: "News",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_News_Images_PhotoId",
                table: "News",
                column: "PhotoId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_Images_PhotoId",
                table: "News");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "News",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "ShortText",
                table: "News",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddForeignKey(
                name: "FK_News_Images_PhotoId",
                table: "News",
                column: "PhotoId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
