﻿using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Common;
using Monifier.BusinessLogic.Model.Accounts;
using Monifier.BusinessLogic.Model.Base;

namespace Monifier.BusinessLogic.Contract.Base
{
    public interface IAccountCommands : ICommonModelCommands<AccountModel>
    {
        Task Topup(TopupAccountModel topup);
        Task Transfer(int accountFromId, int accountToId, decimal amount);
    }
}