using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Monifier.Web.Migrations
{
    public partial class Distribution : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountId",
                table: "ExpenseBills",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AvailBalance",
                table: "Accounts",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWithdraw",
                table: "Accounts",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountFlowSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<int>(nullable: false),
                    CanFlow = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountFlowSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountFlowSettings_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Distributions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    DateTime = table.Column<DateTime>(nullable: false),
                    SumFlow = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Distributions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseFlowSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(nullable: false),
                    CanFlow = table.Column<bool>(nullable: false),
                    ExpenseFlowId = table.Column<int>(nullable: false),
                    Rule = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseFlowSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseFlowSettings_ExpenseFlows_ExpenseFlowId",
                        column: x => x.ExpenseFlowId,
                        principalTable: "ExpenseFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DistributionFlows",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Amount = table.Column<decimal>(nullable: false),
                    DistributionId = table.Column<int>(nullable: true),
                    RecipientId = table.Column<int>(nullable: false),
                    SourceId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributionFlows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DistributionFlows_Distributions_DistributionId",
                        column: x => x.DistributionId,
                        principalTable: "Distributions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DistributionFlows_ExpenseFlows_RecipientId",
                        column: x => x.RecipientId,
                        principalTable: "ExpenseFlows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DistributionFlows_Accounts_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBills_AccountId",
                table: "ExpenseBills",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccountFlowSettings_AccountId",
                table: "AccountFlowSettings",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionFlows_DistributionId",
                table: "DistributionFlows",
                column: "DistributionId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionFlows_RecipientId",
                table: "DistributionFlows",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_DistributionFlows_SourceId",
                table: "DistributionFlows",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseFlowSettings_ExpenseFlowId",
                table: "ExpenseFlowSettings",
                column: "ExpenseFlowId");

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseBills_Accounts_AccountId",
                table: "ExpenseBills",
                column: "AccountId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseBills_Accounts_AccountId",
                table: "ExpenseBills");

            migrationBuilder.DropTable(
                name: "AccountFlowSettings");

            migrationBuilder.DropTable(
                name: "DistributionFlows");

            migrationBuilder.DropTable(
                name: "ExpenseFlowSettings");

            migrationBuilder.DropTable(
                name: "Distributions");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseBills_AccountId",
                table: "ExpenseBills");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "ExpenseBills");

            migrationBuilder.DropColumn(
                name: "AvailBalance",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "LastWithdraw",
                table: "Accounts");
        }
    }
}
