using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Monifier.Web.Migrations
{
    public partial class ExpensesFlowCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExpensesFlowProductCategories",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CategoryId = table.Column<int>(nullable: false),
                    ExpensesFlowId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpensesFlowProductCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpensesFlowProductCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpensesFlowProductCategories_ExpenseFlows_ExpensesFlowId",
                        column: x => x.ExpensesFlowId,
                        principalTable: "ExpenseFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpensesFlowProductCategories_CategoryId",
                table: "ExpensesFlowProductCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpensesFlowProductCategories_ExpensesFlowId",
                table: "ExpensesFlowProductCategories",
                column: "ExpensesFlowId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExpensesFlowProductCategories");
        }
    }
}
