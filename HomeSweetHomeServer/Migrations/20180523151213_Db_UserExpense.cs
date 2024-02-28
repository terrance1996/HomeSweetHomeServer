using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace HomeSweetHomeServer.Migrations
{
    public partial class Db_UserExpense : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthorId = table.Column<int>(nullable: true),
                    Content = table.Column<string>(nullable: true),
                    Cost = table.Column<double>(nullable: true),
                    EType = table.Column<int>(nullable: true),
                    HomeId = table.Column<int>(nullable: true),
                    LastUpdated = table.Column<DateTime>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expense_User_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Expense_Home_HomeId",
                        column: x => x.HomeId,
                        principalTable: "Home",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserExpense",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ExpenseId = table.Column<int>(nullable: true),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserExpense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserExpense_Expense_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expense",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserExpense_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AuthorId",
                table: "Expense",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_HomeId",
                table: "Expense",
                column: "HomeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserExpense_ExpenseId",
                table: "UserExpense",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserExpense_UserId",
                table: "UserExpense",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserExpense");

            migrationBuilder.DropTable(
                name: "Expense");
        }
    }
}
