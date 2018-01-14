using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Model.Expenses;
using Monifier.Common.Extensions;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Distribution;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Distribution
{
    public class DistributionBoard
    {
        public List<DistributionSource> Accounts { get; set; }
        public List<DistributionRecipient> ExpenseFlows { get; set; }
        public List<DistributionFlowModel> DistributionFlows { get; set; }
        public bool Distributed { get; set; }
        public decimal BaseAmount { get; set; }

        public async Task Load(IUnitOfWork unitOfWork)
        {
            Accounts = await GetAccounts(unitOfWork);;
            ExpenseFlows = await GetFlows(unitOfWork);
            Distributed = false;
            BaseAmount = Accounts.Sum(x => x.Balance);
        }
        
        private static async Task<List<DistributionSource>> GetAccounts(IUnitOfWork unitOfWork)
        {
            var accountQueries = unitOfWork.GetQueryRepository<Account>();
            var settingsQueries = unitOfWork.GetQueryRepository<AccountFlowSettings>();
            
            var accounts = await accountQueries.Query
                .Where(x => !x.IsDeleted && x.AvailBalance > 0)
                .OrderBy(x => x.Number)
                .ToListAsync();

            var ids = accounts.Select(x => x.Id).ToList();
            var flowSettings = await settingsQueries.Query
                .Where(x => ids.Contains(x.AccountId))
                .ToListAsync();
                
            return accounts.Select(x =>
            {
                var settings = flowSettings.FirstOrDefault(s => s.AccountId == x.Id);
                return new DistributionSource
                {
                    Id = x.Id,
                    Name = x.Name,
                    Balance = x.AvailBalance,
                    CanFlow = settings?.CanFlow ?? false
                };
            }).ToList();
        }

        private static async Task<List<DistributionRecipient>> GetFlows(IUnitOfWork unitOfWork) 
        {
            var flowQueries = unitOfWork.GetQueryRepository<ExpenseFlow>();
            var settingsQueries = unitOfWork.GetQueryRepository<ExpenseFlowSettings>();

            var flows = await flowQueries.Query
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Version)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var ids = flows.Select(x => x.Id).ToList();
            var flowSettings = settingsQueries.Query
                .Where(x => ids.Contains(x.ExpenseFlowId))
                .ToList();
            
            return flows.Select(x =>
            {
                var settings = flowSettings.FirstOrDefault(s => s.ExpenseFlowId == x.Id);
                return new DistributionRecipient
                {
                    Id = x.Id,
                    Name = x.Name,
                    Balance = x.Balance,
                    CanFlow = settings?.CanFlow ?? false,
                    Rule = settings?.Rule ?? FlowRule.None,
                    Amount = settings?.Amount ?? 0
                };
            }).ToList();
        }

        public async Task Distribute(IUnitOfWork unitOfWork, IFlowDistributor distributor)
        {
            var accountIds = Accounts.Select(x => x.Id).ToList();
            
            var accountsQueries = unitOfWork.GetQueryRepository<Account>();
            var accounts = await accountsQueries.Query
                .Where(x => accountIds.Contains(x.Id))
                .Select(x => x.ToModel())
                .ToListAsync();
            
            accounts.ForEach(x =>
            {
                var account = Accounts.First(a => a.Id == x.Id);
                account.Balance = x.AvailBalance;
                (x as IFlowEndPoint).FlowRule = new DistributionFlowRule
                {
                    CanFlow = account.CanFlow,
                    Destination = FlowDestination.Source
                };
            });

            var flowIds = ExpenseFlows.Select(x => x.Id).ToList();
            
            var flowQueries = unitOfWork.GetQueryRepository<ExpenseFlow>();
            var flows = await flowQueries.Query
                .Where(x => flowIds.Contains(x.Id))
                .Select(x => x.ToModel())
                .ToListAsync();
            
            flows.ForEach(x =>
            {
                var expenseFlow = ExpenseFlows.First(s => s.Id == x.Id);
                expenseFlow.Balance = x.Balance;
                x.FlowRule = new DistributionFlowRule
                {
                    CanFlow = expenseFlow.CanFlow,
                    Destination = FlowDestination.Recipient,
                    Rule = expenseFlow.Rule,
                    Amount = expenseFlow.Amount
                };
            });

            var endPoints = new List<IFlowEndPoint>();
            endPoints.AddRange(accounts);
            endPoints.AddRange(flows);
            
            distributor.Distribute(endPoints);

            DistributionFlows = distributor.FlowRecords
                .Select(x => new DistributionFlowModel
                {
                    SourceId = x.Source.Id,
                    SourceName = x.Source.Name,
                    RecipientId = x.Recipient.Id,
                    RecipientName = x.Recipient.Name,
                    Amount = x.Amount,
                    AmountFormatted = x.Amount.ToMoney()
                }).ToList();

            Accounts.ForEach(x =>
            {
                if (!x.CanFlow) return;
                var withdrawTotal = distributor.FlowRecords.Where(f => f.Source.Id == x.Id).Sum(f => f.Amount);
                if (withdrawTotal > 0) x.WithdrawTotal = $"\u2013{withdrawTotal.ToMoney()}";
                x.Result = (x.Balance - withdrawTotal).ToMoney();
            });
            
            ExpenseFlows.ForEach(x =>
            {
                if (!x.CanFlow) return;
                var topupTotal = distributor.FlowRecords.Where(f => f.Recipient.Id == x.Id).Sum(f => f.Amount); 
                if (topupTotal > 0) x.TopupTotal = $"+{topupTotal.ToMoney()}";
                x.Result = (x.Balance + topupTotal).ToMoney();
            });
        }

        public async Task Save(IUnitOfWork unitOfWork)
        {
            var accountSettingsQueries = unitOfWork.GetQueryRepository<AccountFlowSettings>();
            var accountSettingsCommands = unitOfWork.GetCommandRepository<AccountFlowSettings>();
            
            var accountIds = Accounts.Select(x => x.Id).ToList();
            var accountSettings = await accountSettingsQueries.Query
                .Where(x => accountIds.Contains(x.AccountId))
                .ToListAsync();
            
            Accounts.ForEach(x =>
            {
                var settings = accountSettings.FirstOrDefault(s => s.AccountId == x.Id);
                if (settings == null)
                {
                    settings = new AccountFlowSettings
                    {
                        AccountId = x.Id,
                        CanFlow = x.CanFlow
                    };
                    accountSettingsCommands.Create(settings);
                }
                else
                {
                    settings.CanFlow = x.CanFlow;
                    accountSettingsCommands.Update(settings);
                }
            });

            var flowSettingsQueries = unitOfWork.GetQueryRepository<ExpenseFlowSettings>();
            var flowSettingsCommands = unitOfWork.GetCommandRepository<ExpenseFlowSettings>();

            var flowIds = ExpenseFlows.Select(x => x.Id).ToList();
            var flowSettings = await flowSettingsQueries.Query
                .Where(x => flowIds.Contains(x.ExpenseFlowId))
                .ToListAsync();
            
            ExpenseFlows.ForEach(x =>
            {
                var settings = flowSettings.FirstOrDefault(s => s.ExpenseFlowId == x.Id);
                if (settings == null)
                {
                    settings = new ExpenseFlowSettings
                    {
                        ExpenseFlowId = x.Id,
                        CanFlow = x.CanFlow,
                        Rule = x.Rule,
                        Amount = x.Amount
                    };
                    flowSettingsCommands.Create(settings);
                }
                else
                {
                    settings.CanFlow = x.CanFlow;
                    settings.Rule = x.Rule;
                    settings.Amount = x.Amount;
                    flowSettingsCommands.Update(settings);
                }
            });

            var distribution = new DataAccess.Model.Distribution.Distribution()
            {
                DateTime = DateTime.Now,
                SumFlow = DistributionFlows.Sum(x => x.Amount)
            };
            unitOfWork.GetCommandRepository<DataAccess.Model.Distribution.Distribution>().Create(distribution);

            var flowsCommands = unitOfWork.GetCommandRepository<Flow>();
            var accountQueries = unitOfWork.GetQueryRepository<Account>();
            var accountCommands = unitOfWork.GetCommandRepository<Account>();
            var flowQueries = unitOfWork.GetQueryRepository<ExpenseFlow>();
            var expenseFlowCommands = unitOfWork.GetCommandRepository<ExpenseFlow>();
            
            var accounts = await accountQueries.Query
                .Where(x => accountIds.Contains(x.Id))
                .ToListAsync();
            var expenseFlows = await flowQueries.Query
                .Where(x => flowIds.Contains(x.Id))
                .ToListAsync();
            
            DistributionFlows.ForEach(x =>
            {
                var flow = new Flow
                {
                    Distribution = distribution,
                    SourceId = x.SourceId,
                    RecipientId = x.RecipientId,
                    Amount = x.Amount
                };
                flowsCommands.Create(flow);

                var account = accounts.First(a => a.Id == x.SourceId);
                var expenseFlow = expenseFlows.First(f => f.Id == x.RecipientId);

                account.AvailBalance -= x.Amount;
                accountCommands.Update(account);
                expenseFlow.Balance += x.Amount;
                expenseFlowCommands.Update(expenseFlow);
            });
            
            await unitOfWork.SaveChangesAsync();
        }
    }
}