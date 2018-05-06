﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Monifier.DataAccess.EntityFramework;
using Monifier.DataAccess.Model.Distribution;
using System;

namespace Monifier.Web.Migrations
{
    [DbContext(typeof(MonifierDbContext))]
    partial class MonifierDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.1-rtm-125")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Monifier.DataAccess.Model.Accounts.Transactions", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("BillId");

                    b.Property<DateTime>("DateTime");

                    b.Property<int?>("IncomeId");

                    b.Property<int>("InitiatorId");

                    b.Property<bool>("IsDeleted");

                    b.Property<int?>("OwnerId");

                    b.Property<int?>("ParticipantId");

                    b.Property<decimal>("Total");

                    b.HasKey("Id");

                    b.HasIndex("BillId");

                    b.HasIndex("IncomeId");

                    b.HasIndex("InitiatorId");

                    b.HasIndex("OwnerId");

                    b.HasIndex("ParticipantId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Auth.Session", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<DateTime?>("Expiration");

                    b.Property<bool>("IsAdmin");

                    b.Property<Guid>("Token");

                    b.Property<int>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("Token")
                        .IsUnique();

                    b.HasIndex("UserId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Auth.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Hash");

                    b.Property<bool>("IsAdmin");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Login");

                    b.Property<string>("Name");

                    b.Property<string>("Salt");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Base.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("AvailBalance");

                    b.Property<decimal>("Balance");

                    b.Property<DateTime>("DateCreated");

                    b.Property<bool>("IsDefault");

                    b.Property<bool>("IsDeleted");

                    b.Property<DateTime?>("LastWithdraw");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("Number");

                    b.Property<int?>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Base.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int?>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Base.Product", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CategoryId");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int?>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Distribution.AccountFlowSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountId");

                    b.Property<bool>("CanFlow");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("AccountFlowSettings");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Distribution.Distribution", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateTime");

                    b.Property<decimal>("SumFlow");

                    b.HasKey("Id");

                    b.ToTable("Distributions");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Distribution.ExpenseFlowSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<bool>("CanFlow");

                    b.Property<int>("ExpenseFlowId");

                    b.Property<int>("Rule");

                    b.HasKey("Id");

                    b.HasIndex("ExpenseFlowId");

                    b.ToTable("ExpenseFlowSettings");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Distribution.Flow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<int>("DistributionId");

                    b.Property<int>("RecipientId");

                    b.Property<int>("SourceId");

                    b.HasKey("Id");

                    b.HasIndex("DistributionId");

                    b.HasIndex("RecipientId");

                    b.HasIndex("SourceId");

                    b.ToTable("DistributionFlows");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpenseBill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("AccountId");

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("ExpenseFlowId");

                    b.Property<bool>("IsCorrection");

                    b.Property<int?>("OwnerId");

                    b.Property<decimal>("SumPrice");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("ExpenseFlowId");

                    b.HasIndex("OwnerId");

                    b.ToTable("ExpenseBills");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpenseFlow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Balance");

                    b.Property<DateTime>("DateCreated");

                    b.Property<bool>("IsDeleted");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("Number");

                    b.Property<int?>("OwnerId");

                    b.Property<int>("Version");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("ExpenseFlows");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpenseItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("BillId");

                    b.Property<int?>("CategoryId");

                    b.Property<string>("Comment");

                    b.Property<decimal>("Price");

                    b.Property<int?>("ProductId");

                    b.Property<decimal?>("Quantity");

                    b.HasKey("Id");

                    b.HasIndex("BillId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ProductId");

                    b.ToTable("ExpenseItems");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpensesFlowProductCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CategoryId");

                    b.Property<int>("ExpensesFlowId");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ExpensesFlowId");

                    b.ToTable("ExpensesFlowProductCategories");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Incomes.IncomeItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccountId");

                    b.Property<DateTime>("DateTime");

                    b.Property<int>("IncomeTypeId");

                    b.Property<bool>("IsCorrection");

                    b.Property<int?>("OwnerId");

                    b.Property<decimal>("Total");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("IncomeTypeId");

                    b.HasIndex("OwnerId");

                    b.ToTable("IncomeItems");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Incomes.IncomeType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int?>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("IncomeTypes");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Accounts.Transactions", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Expenses.ExpenseBill", "Bill")
                        .WithMany()
                        .HasForeignKey("BillId");

                    b.HasOne("Monifier.DataAccess.Model.Incomes.IncomeItem", "Income")
                        .WithMany()
                        .HasForeignKey("IncomeId");

                    b.HasOne("Monifier.DataAccess.Model.Base.Account", "Initiator")
                        .WithMany()
                        .HasForeignKey("InitiatorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.HasOne("Monifier.DataAccess.Model.Base.Account", "Participant")
                        .WithMany()
                        .HasForeignKey("ParticipantId");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Auth.Session", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Base.Account", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Base.Category", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Base.Product", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Base.Category", "Category")
                        .WithMany("Products")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Distribution.AccountFlowSettings", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Base.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Distribution.ExpenseFlowSettings", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Expenses.ExpenseFlow", "ExpenseFlow")
                        .WithMany()
                        .HasForeignKey("ExpenseFlowId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Distribution.Flow", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Distribution.Distribution", "Distribution")
                        .WithMany("Flows")
                        .HasForeignKey("DistributionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Expenses.ExpenseFlow", "Recipient")
                        .WithMany()
                        .HasForeignKey("RecipientId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Base.Account", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpenseBill", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Base.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId");

                    b.HasOne("Monifier.DataAccess.Model.Expenses.ExpenseFlow", "ExpenseFlow")
                        .WithMany()
                        .HasForeignKey("ExpenseFlowId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpenseFlow", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpenseItem", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Expenses.ExpenseBill", "Bill")
                        .WithMany("Items")
                        .HasForeignKey("BillId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Base.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("Monifier.DataAccess.Model.Base.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Expenses.ExpensesFlowProductCategory", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Base.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Expenses.ExpenseFlow", "ExpenseFlow")
                        .WithMany()
                        .HasForeignKey("ExpensesFlowId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Incomes.IncomeItem", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Base.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Incomes.IncomeType", "IncomeType")
                        .WithMany()
                        .HasForeignKey("IncomeTypeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Monifier.DataAccess.Model.Incomes.IncomeType", b =>
                {
                    b.HasOne("Monifier.DataAccess.Model.Auth.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });
#pragma warning restore 612, 618
        }
    }
}
