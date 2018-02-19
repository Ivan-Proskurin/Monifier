using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Monifier.Web.Migrations
{
    public partial class Auth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DistributionFlows_Distributions_DistributionId",
                table: "DistributionFlows");

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "IncomeTypes",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "IncomeItems",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "ExpenseFlows",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "ExpenseBills",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DistributionId",
                table: "DistributionFlows",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Categories",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Accounts",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Hash = table.Column<string>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Login = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Salt = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(nullable: false),
                    Expiration = table.Column<DateTime>(nullable: true),
                    IsAdmin = table.Column<bool>(nullable: false),
                    Token = table.Column<Guid>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_OwnerId",
                table: "Products",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeTypes_OwnerId",
                table: "IncomeTypes",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeItems_OwnerId",
                table: "IncomeItems",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseFlows_OwnerId",
                table: "ExpenseFlows",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseBills_OwnerId",
                table: "ExpenseBills",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_OwnerId",
                table: "Categories",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_OwnerId",
                table: "Accounts",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_Token",
                table: "Sessions",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_UserId",
                table: "Sessions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_OwnerId",
                table: "Accounts",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_OwnerId",
                table: "Categories",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionFlows_Distributions_DistributionId",
                table: "DistributionFlows",
                column: "DistributionId",
                principalTable: "Distributions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseBills_Users_OwnerId",
                table: "ExpenseBills",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ExpenseFlows_Users_OwnerId",
                table: "ExpenseFlows",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomeItems_Users_OwnerId",
                table: "IncomeItems",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_IncomeTypes_Users_OwnerId",
                table: "IncomeTypes",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Users_OwnerId",
                table: "Products",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_OwnerId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_OwnerId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_DistributionFlows_Distributions_DistributionId",
                table: "DistributionFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseBills_Users_OwnerId",
                table: "ExpenseBills");

            migrationBuilder.DropForeignKey(
                name: "FK_ExpenseFlows_Users_OwnerId",
                table: "ExpenseFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomeItems_Users_OwnerId",
                table: "IncomeItems");

            migrationBuilder.DropForeignKey(
                name: "FK_IncomeTypes_Users_OwnerId",
                table: "IncomeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Users_OwnerId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Sessions");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Products_OwnerId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_IncomeTypes_OwnerId",
                table: "IncomeTypes");

            migrationBuilder.DropIndex(
                name: "IX_IncomeItems_OwnerId",
                table: "IncomeItems");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseFlows_OwnerId",
                table: "ExpenseFlows");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseBills_OwnerId",
                table: "ExpenseBills");

            migrationBuilder.DropIndex(
                name: "IX_Categories_OwnerId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_OwnerId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "IncomeTypes");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "IncomeItems");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ExpenseFlows");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "ExpenseBills");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Accounts");

            migrationBuilder.AlterColumn<int>(
                name: "DistributionId",
                table: "DistributionFlows",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_DistributionFlows_Distributions_DistributionId",
                table: "DistributionFlows",
                column: "DistributionId",
                principalTable: "Distributions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
