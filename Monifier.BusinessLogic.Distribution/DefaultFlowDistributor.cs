using System;
using System.Collections.Generic;
using System.Linq;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Model.Distribution;

namespace Monifier.BusinessLogic.Distribution
{
    public class DefaultFlowDistributor : IFlowDistributor
    {
        public void Distribute(IEnumerable<IFlowEndPoint> endPoints)
        {
            ValidateAndSplit(endPoints);
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

            _sources = _endPoints.Where(x => x.FlowRule.CanFlow && 
                                             (x.FlowRule.Destination == FlowDestination.Source && x.Balance > 0 ||
                                              x.FlowRule.Destination == FlowDestination.Both)).ToList();
            _recipients = _endPoints.Where(x => x.FlowRule.CanFlow && 
                                                (x.FlowRule.Destination == FlowDestination.Recipient ||
                                                 x.FlowRule.Destination == FlowDestination.Both)).ToList();

            if (_sources.Count == 0 && _recipients.Count > 0)
                throw new FlowDistributionSchemaException(_endPoints, "No source points with no zero recipients");
            
            if (_sources.Sum(x => x.Balance) <= 0 && _recipients.Count > 0)
                throw new FlowDistributionSchemaException(_endPoints, "Total balance of all sources must be positive");
        }

        private void PrepareToDistribute()
        {
            _sources = _sources.OrderBy(x => x.FlowRule.Destination).ThenByDescending(x => x.Balance).ToList();
            _recipients = _recipients.OrderByDescending(x => x.FlowRule.Destination).ThenBy(x => x.FlowRule.Rule)
                .ThenByDescending(x => x.FlowRule.Amount).ThenBy(x => x.Id).ToList();
        }

        private void RecordFlowsEntries()
        {
            var totalRest = _sources.Where(x => x.Balance > 0).Sum(x => x.Balance);
            foreach (var recipient in _recipients)
            {
                var amount = ApplyRule(totalRest, recipient.FlowRule);
                if (amount > totalRest)
                    throw new FlowDistributionException(_sources, recipient, amount);

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
                    else if (source.Balance > 0)
                    {
                        var flowRec = new DistributionFlow(source, recipient, source.Balance);
                        _flowRecords.Add(flowRec);
                        var withdrawValue = source.Balance;
                        amount -= withdrawValue;
                        source.Withdraw(withdrawValue);
                        recipient.Topup(withdrawValue);
                    }
                }

                if (recipient.FlowRule.Destination == FlowDestination.Both && recipient.Balance > 0)
                    totalRest += recipient.Balance;
            }
        }

        private decimal? allRestValue = null;
        private decimal ApplyRule(decimal value, DistributionFlowRule flowRule)
        {
            var rule = flowRule.Rule;
            var amount = flowRule.Amount;
            switch (rule)
            {
                case FlowRule.None:
                    return 0;
                case FlowRule.FixedFromBase:
                case FlowRule.FixedFromRest:
                    return amount;
                case FlowRule.PercentFromBase:
                case FlowRule.PercentFromRest:
                    return value * amount / 100;
                case FlowRule.AllRest:
                    if (allRestValue != null) return allRestValue.Value;
                    allRestValue = value / _recipients.Count(x => x.FlowRule.Rule == FlowRule.AllRest);
                    return allRestValue.Value;
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
        private List<DistributionFlow> _flowRecords = new List<DistributionFlow>();
        public IList<DistributionFlow> FlowRecords => _flowRecords;
    }
}