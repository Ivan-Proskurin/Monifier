using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Monifier.Web.Migrations
{
    public partial class RemoveAccountFromExpenseFlow : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseBills_Accounts_AccountId",
                table: "ExpenseBills");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseBills_AccountId",
                table: "ExpenseBills");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExpenseBills");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExpenseBills",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBills_AccountId",
                table: "ExpenseBills",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseBills_Accounts_AccountId",
                table: "ExpenseBills",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
