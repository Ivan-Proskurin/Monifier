using System.Collections.Generic;
using Monifier.BusinessLogic.Model.Agregation;

namespace Monifier.BusinessLogic.Model.Base
{
    public class AccountList
    {
        public List<AccountModel> Accounts { get; set; }
        public AccountsTotals Totals { get; set; }
    }
}