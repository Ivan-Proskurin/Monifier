using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Monifier.Web.Migrations
{
    public partial class CorrectBillsItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "ExpenseItems");

            migrationBuilder.AddColumn<int>(
                name: "ExpenseFlowId",
                table: "ExpenseBills",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBills_ExpenseFlowId",
                table: "ExpenseBills",
                column: "ExpenseFlowId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseBills_ExpenseFlows_ExpenseFlowId",
                table: "ExpenseBills",
                column: "ExpenseFlowId",
                principalTable: "ExpenseFlows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseBills_ExpenseFlows_ExpenseFlowId",
                table: "ExpenseBills");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseBills_ExpenseFlowId",
                table: "ExpenseBills");

            migrationBuilder.DropColumn(
                name: "ExpenseFlowId",
                table: "ExpenseBills");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "ExpenseItems",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
