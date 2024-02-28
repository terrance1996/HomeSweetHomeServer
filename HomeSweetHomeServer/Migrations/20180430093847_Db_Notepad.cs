using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace HomeSweetHomeServer.Migrations
{
    public partial class Db_Notepad : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Home_HomeId",
                table: "User");

            migrationBuilder.CreateTable(
                name: "Notepad",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Content = table.Column<string>(nullable: true),
                    HomeId = table.Column<int>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notepad", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notepad_Home_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Home",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notepad_HomeId",
                table: "Notepad",
                column: "HomeId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Home_HomeId",
                table: "User",
                column: "HomeId",
                principalTable: "Home",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Home_HomeId",
                table: "User");

            migrationBuilder.DropTable(
                name: "Notepad");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Home_HomeId",
                table: "User",
                column: "HomeId",
                principalTable: "Home",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
