using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Auth;
using Monifier.DataAccess.EntityFramework;

namespace Monifier.Tools.Accounts
{
    class AccountInfo
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
    }

    class Program
    {
        private static readonly string ConnectionString =
            "Data Source=.\\SQLEXPRESS;Initial Catalog=Monifier;Integrated Security=True;MultipleActiveResultSets=True";
//        private static readonly string ConnectionString =
//            "Data Source=.\\SQLEXPRESS;Initial Catalog=Moneyflow;Integrated Security=True;MultipleActiveResultSets=True";

        private static readonly AccountInfo Account = new AccountInfo
        {
            Name = "Иван",
            Login = "exosyphen",
            Password = "[bnhsqgfhjkm",
            IsAdmin = true,
        };
        
        static void Main()
        {
            var contextOptions = new DbContextOptionsBuilder<MonifierDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;
            
            using (var context = new MonifierDbContext(contextOptions))
            {
                using (var unitOfWork = new UnitOfWork(context))
                {
                    var authCommands = new AuthCommands(unitOfWork);
                    authCommands.CreateUser(Account.Name, Account.Login, Account.Password, Account.IsAdmin).Wait();
                }
            }
        }
    }
}