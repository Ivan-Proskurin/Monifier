using Microsoft.EntityFrameworkCore;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Distribution;
using Monifier.DataAccess.Model.Expenses;
using Monifier.DataAccess.Model.Incomes;

namespace Monifier.DataAccess.EntityFramework
{
    public class MonifierDbContext : DbContext
    {
        public MonifierDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ExpenseItem> ExpenseItems { get; set; }
        public DbSet<ExpenseBill> ExpenseBills { get; set; }
        public DbSet<IncomeType> IncomeTypes { get; set; }
        public DbSet<IncomeItem> IncomeItems { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<ExpenseFlow> ExpenseFlows { get; set; }
        public DbSet<ExpensesFlowProductCategory> ExpensesFlowProductCategories { get; set; }
        public DbSet<Distribution> Distributions { get; set; }
        public DbSet<Flow> DistributionFlows { get; set; }
        public DbSet<AccountFlowSettings> AccountFlowSettings { get; set; } 
        public DbSet<ExpenseFlowSettings> ExpenseFlowSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExpenseItem>()
                .HasOne(e => e.Product)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
        
    }
}
