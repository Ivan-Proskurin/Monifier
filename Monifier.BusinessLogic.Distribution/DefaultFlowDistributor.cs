using System;
using System.Collections.Generic;
using System.Linq;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.DataAccess.Model.Distribution;

namespace Monifier.BusinessLogic.Distribution
{
    public class DefaultFlowDistributor : IFlowDistributor
    {
        public void Distribute(IEnumerable<IFlowEndPoint> endPoints)
        {
            ValidateAndSplit(endPoints);
            CalculateRecipientWants();
            PrepareToDistribute();
            try
            {
                RecordFlowsEntries();
            }
            catch
            {
                RollbackFlowRecords();
                throw;
            }
        }

        private void ValidateAndSplit(IEnumerable<IFlowEndPoint> endPoints)
        {
            if (endPoints == null)
                throw new ArgumentNullException(nameof(endPoints));
            
            _endPoints = endPoints.ToList();
            if (_endPoints.Count == 0)
                throw new InvalidOperationException("endPoints must be non empty collection");

            _sources = _endPoints.Where(x => x.FlowRule.CanFlow  && x.Balance > 0 &&
                                             x.FlowRule.Destination == FlowDestination.Source).ToList();
            _recipients = _endPoints.Where(x => x.FlowRule.CanFlow && x.FlowRule.Rule != FlowRule.None &&
                                                x.FlowRule.Destination == FlowDestination.Recipient).ToList();

            if (_sources.Count == 0 && _recipients.Count > 0)
                throw new FlowDistributionSchemaException(_endPoints, "No source points with non zero recipients");

            if (_sources.Sum(x => x.Balance) <= 0 && _recipients.Count > 0)
                throw new FlowDistributionSchemaException(_endPoints, "Total balance of all sources must be positive");
        }

        private void CalculateRecipientWants()
        {
            _recipientWants.Clear();
            var fixedRecipients = _recipients.Where(x => x.FlowRule.Rule == FlowRule.FixedFromBase).ToList();
            var sumBalance = _sources.Sum(x => x.Balance) - fixedRecipients.Sum(x => x.FlowRule.Amount);
            
            if (sumBalance < 0)
                throw new FlowDistributionSchemaException(_endPoints, "Сумма значений фиксированных правил превышает базовую сумму");
            
            fixedRecipients.ForEach(x =>
            {
                _recipientWants.Add(x.Id, x.FlowRule.Amount);
            });

            var percentRecipients = _recipients.Where(x => x.FlowRule.Rule == FlowRule.PercentFromBase).ToList();
            if (percentRecipients.Sum(x => x.FlowRule.Amount) > 100)
                throw new FlowDistributionSchemaException(_endPoints, "Сумма процентных правил превышает 100%");

            var percentSum = 0m;
            percentRecipients.ForEach(x =>
            {
                var amount = Math.Round(sumBalance * x.FlowRule.Amount / 100, 2, MidpointRounding.AwayFromZero);
                _recipientWants.Add(x.Id, amount);
                percentSum += amount;
            });

            var restRecipients = _recipients.Where(x => x.FlowRule.Rule == FlowRule.AllRest).ToList();
            var restAmount = restRecipients.Count > 0 ? (sumBalance - percentSum) / restRecipients.Count : 0;
            restRecipients.ForEach(x =>
            {
                _recipientWants.Add(x.Id, restAmount);
            });
        }
        
        private void PrepareToDistribute()
        {
            _sources = _sources.OrderBy(x => x.FlowRule.Destination).ThenByDescending(x => x.Balance).ToList();
            _recipients = _recipients.OrderByDescending(x => x.FlowRule.Destination).ThenBy(x => x.FlowRule.Rule)
                .ThenByDescending(x => x.FlowRule.Amount).ThenBy(x => x.Id).ToList();
        }

        private void RecordFlowsEntries()
        {
            var totalRest = _sources.Sum(x => x.Balance);
            foreach (var recipient in _recipients)
            {
                var amount = _recipientWants[recipient.Id];
                if (amount > totalRest) amount = totalRest;

                totalRest -= amount;
                
                foreach (var source in _sources)
                {
                    if (source.Balance >= amount)
                    {
                        var flowRec = new DistributionFlow(source, recipient, amount);
                        _flowRecords.Add(flowRec);
                        source.Withdraw(amount);
                        recipient.Topup(amount);
                        break;
                    }
                    
                    if (source.Balance > 0)
                    {
                        var flowRec = new DistributionFlow(source, recipient, source.Balance);
                        _flowRecords.Add(flowRec);
                        var withdrawValue = source.Balance;
                        amount -= withdrawValue;
                        source.Withdraw(withdrawValue);
                        recipient.Topup(withdrawValue);
                    }
                }
            }
        }

        private decimal? _allRestValue;
        private decimal ApplyRule(decimal value, DistributionFlowRule flowRule)
        {
            var rule = flowRule.Rule;
            var amount = flowRule.Amount;
            switch (rule)
            {
                case FlowRule.None:
                    return 0;
                case FlowRule.FixedFromBase:
                    return amount;
                case FlowRule.PercentFromBase:
                    return Math.Round(value * amount / 100, 2, MidpointRounding.AwayFromZero);
                case FlowRule.AllRest:
                    if (_allRestValue != null)
                    {
                        return _allRestValue.Value > value ? value : _allRestValue.Value; 
                    }
                    _allRestValue = value / _recipients.Count(x => x.FlowRule.Rule == FlowRule.AllRest);
                    _allRestValue = Math.Round(_allRestValue.Value, 2, MidpointRounding.AwayFromZero);
                    return _allRestValue.Value;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rule), rule, null);
            }
        }

        private void RollbackFlowRecords()
        {
            foreach (var record in _flowRecords)
            {
                record.Source.Topup(record.Amount);
                record.Recipient.Withdraw(record.Amount);
            }
            _flowRecords.Clear();
        }

        private List<IFlowEndPoint> _endPoints;
        private List<IFlowEndPoint> _sources;
        private List<IFlowEndPoint> _recipients;
        private readonly Dictionary<int, decimal> _recipientWants = new Dictionary<int, decimal>();
        private readonly List<DistributionFlow> _flowRecords = new List<DistributionFlow>();
        public IList<DistributionFlow> FlowRecords => _flowRecords;
    }
}