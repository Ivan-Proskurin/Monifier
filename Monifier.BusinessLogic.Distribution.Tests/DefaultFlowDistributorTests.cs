using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.BusinessLogic.Distribution.Model.Contract;
using Monifier.BusinessLogic.Distribution.Tests.Models;

namespace Monifier.BusinessLogic.Distribution.Tests
{
    [TestClass]
    public class DefaultFlowDistributorTests
    {
        private EndPointTestSchema _schema;

        [TestInitialize]
        public void Setup()
        {
            _schema = new EndPointTestSchema();
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void EmptyPointsDistribution_InvalidOperationThrows()
        {
            var target = new DefaultFlowDistributor();
            target.Distribute(Enumerable.Empty<IFlowEndPoint>());
        }

        [TestMethod]
        public void NoNeedToDistribute_NoErrorsEmptyRecords()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.NotParticipantSourcePoint,
                _schema.NotParticipantRecipientPoint
            });
            
            Assert.IsNotNull(target.FlowRecords);
            Assert.AreEqual(0, target.FlowRecords.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(FlowDistributionSchemaException))]
        public void NoSourcesEndPoints_ThrowsSchemaError()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.NotParticipantSourcePoint,
                _schema.RecipientPoint_FixedFromBase1000,
                _schema.RecipientPoint_FixedFromBase500
            });
        }

        [TestMethod]
        public void NoRecipientEndPoints_NoErrorsEmptyRecords()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.SourcePoint_5000Balance,
                _schema.SourcePoint_ZeroBalance
            });
            
            Assert.IsNotNull(target.FlowRecords);
            Assert.AreEqual(0, target.FlowRecords.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(FlowDistributionSchemaException))]
        public void NoSourceWithPositiveBalance_ThowsSchemaError()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.SourcePoint_ZeroBalance,
                _schema.RecipientPoint_FixedFromBase1000,
                _schema.NotParticipantSourcePoint
            });
        }

        [TestMethod]
        public void CommonBalanceIsNegative_ThrowsErrorEmptyRecords()
        {
            var target = new DefaultFlowDistributor();

            try
            {
                target.Distribute(new[]
                {
                    _schema.SourcePoint_ZeroBalance,
                    _schema.SourcePoint_5000Balance,
                    _schema.SourcePoint_1000Balance,
                    _schema.RecipientPoint_FixedFromBase500,
                    _schema.RecipientPoint_FixedFromBase3000,
                    _schema.RecipientPoint_FixedFromBase1000,
                    _schema.RecipientPoint_FixedFromBase2500,
                    _schema.RecipientPoint_PercentFromBase25,
                });
                Assert.Fail("Ожидался выброс FlowDistributionException");
            }
            catch (FlowDistributionSchemaException)
            {
            }
            
            Assert.AreEqual(0, target.FlowRecords.Count);
            Assert.AreEqual(5000, _schema.SourcePoint_5000Balance.Balance);
            Assert.AreEqual(1000, _schema.SourcePoint_1000Balance.Balance);
            Assert.AreEqual(0, _schema.SourcePoint_ZeroBalance.Balance);
            Assert.AreEqual(10, _schema.RecipientPoint_FixedFromBase500.Balance);
            Assert.AreEqual(100, _schema.RecipientPoint_FixedFromBase3000.Balance);
            Assert.AreEqual(300, _schema.RecipientPoint_FixedFromBase1000.Balance);
            Assert.AreEqual(700, _schema.RecipientPoint_PercentFromBase25.Balance);
        }

        [TestMethod]
        public void NormalBalanceToZero_DistributionSuccess()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new[]
            {
                _schema.SourcePoint_ZeroBalance,
                _schema.SourcePoint_5000Balance,
                _schema.RecipientPoint_FixedFromBase3000,
                _schema.RecipientPoint_FixedFromBase1000,
                _schema.RecipientPoint_PercentFromBase25,
                _schema.RecipientPoint_AllRest1
            });
            
            Assert.AreEqual(4, target.FlowRecords.Count);
            var flow1 = target.FlowRecords[0];
            var flow2 = target.FlowRecords[1];
            var flow3 = target.FlowRecords[2];
            var flow4 = target.FlowRecords[3];
            
            Assert.AreEqual(0, _schema.SourcePoint_5000Balance.Balance);
            
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow1.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase3000.Name, flow1.Recipient.Name);
            Assert.AreEqual(3000, flow1.Amount);
            Assert.AreEqual(3100, _schema.RecipientPoint_FixedFromBase3000.Balance);
            
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow2.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase1000.Name, flow2.Recipient.Name);
            Assert.AreEqual(1000, flow2.Amount);
            Assert.AreEqual(1300, _schema.RecipientPoint_FixedFromBase1000.Balance);
            
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow3.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_PercentFromBase25.Name, flow3.Recipient.Name);
            Assert.AreEqual(250, flow3.Amount);
            Assert.AreEqual(950, _schema.RecipientPoint_PercentFromBase25.Balance);

            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow4.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_AllRest1.Name, flow4.Recipient.Name);
            Assert.AreEqual(750, flow4.Amount);
            Assert.AreEqual(750, _schema.RecipientPoint_AllRest1.Balance);
        }

        [TestMethod]
        public void MultipleSourcesBalanceDivision_DistributionSuccess()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.SourcePoint_5000Balance,
                _schema.SourcePoint_1000Balance,
                _schema.RecipientPoint_FixedFromBase3000,
                _schema.RecipientPoint_FixedFromBase2500
            });
            
            Assert.AreEqual(3, target.FlowRecords.Count);
            var flow1 = target.FlowRecords[0];
            var flow2 = target.FlowRecords[1];
            var flow3 = target.FlowRecords[2];
            
            Assert.AreEqual(0, _schema.SourcePoint_5000Balance.Balance);
            Assert.AreEqual(500, _schema.SourcePoint_1000Balance.Balance);
            
            // flow1
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow1.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase3000.Name, flow1.Recipient.Name);
            Assert.AreEqual(3000, flow1.Amount);
            Assert.AreEqual(3100, _schema.RecipientPoint_FixedFromBase3000.Balance);
            
            // flow2
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow2.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase2500.Name, flow2.Recipient.Name);
            Assert.AreEqual(2000, flow2.Amount);

            // flow3
            Assert.AreEqual(_schema.SourcePoint_1000Balance.Name, flow3.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase2500.Name, flow3.Recipient.Name);
            Assert.AreEqual(500, flow3.Amount);
            Assert.AreEqual(3400, _schema.RecipientPoint_FixedFromBase2500.Balance);
        }

        // Тестовый сценарий Case1.odt
        [TestMethod]
        public void ComplexDistribution_DistributionSuccess()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.SourcePoint_ZeroBalance,
                _schema.NotParticipantSourcePoint,
                _schema.SourcePoint_5000Balance,
                _schema.SourcePoint_1000Balance,
                _schema.RecipientPoint_FixedFromBase1000, // 300
                _schema.RecipientPoint_PercentFromBase30, // 450
                _schema.RecipientPoint_PercentFromBase25, // 700
                _schema.RecipientPoint_AllRest1, // 0
                _schema.RecipientPoint_AllRest2, // 100
                _schema.NotParticipantRecipientPoint,
                _schema.RecipientPoint_FixedFromBase2500,
            });
            
            Assert.AreEqual(7, target.FlowRecords.Count);
            var flow1 = target.FlowRecords[0];
            var flow2 = target.FlowRecords[1];
            var flow3 = target.FlowRecords[2];
            var flow4 = target.FlowRecords[3];
            var flow5 = target.FlowRecords[4];
            var flow6 = target.FlowRecords[5];
            var flow7 = target.FlowRecords[6];
            
            Assert.AreEqual(0, _schema.SourcePoint_5000Balance.Balance);
            Assert.AreEqual(0, _schema.SourcePoint_1000Balance.Balance);
            
            // flow1
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow1.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase2500.Name, flow1.Recipient.Name);
            Assert.AreEqual(2500, flow1.Amount);
            Assert.AreEqual(3400, _schema.RecipientPoint_FixedFromBase2500.Balance);
            
            // flow2
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow2.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase1000.Name, flow2.Recipient.Name);
            Assert.AreEqual(1000, flow2.Amount);
            Assert.AreEqual(1300, _schema.RecipientPoint_FixedFromBase1000.Balance);

            // flow3
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow3.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_PercentFromBase30.Name, flow3.Recipient.Name);
            Assert.AreEqual(750, flow3.Amount);
            Assert.AreEqual(1200, _schema.RecipientPoint_PercentFromBase30.Balance);
            
            // flow4
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow4.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_PercentFromBase25.Name, flow4.Recipient.Name);
            Assert.AreEqual(625, flow4.Amount);
            Assert.AreEqual(1325, _schema.RecipientPoint_PercentFromBase25.Balance);
            
            // flow5
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow5.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_AllRest1.Name, flow5.Recipient.Name);
            Assert.AreEqual(125, flow5.Amount);
            
            // flow6
            Assert.AreEqual(_schema.SourcePoint_1000Balance.Name, flow6.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_AllRest1.Name, flow6.Recipient.Name);
            Assert.AreEqual(437.5m, flow6.Amount);
            Assert.AreEqual(562.5m, _schema.RecipientPoint_AllRest1.Balance);
         
            // flow7
            Assert.AreEqual(_schema.SourcePoint_1000Balance.Name, flow7.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_AllRest2.Name, flow7.Recipient.Name);
            Assert.AreEqual(562.5m, flow7.Amount);
            Assert.AreEqual(662.5m, _schema.RecipientPoint_AllRest2.Balance);
        }

        [TestMethod]
        public void DistributeRests_DistributeSuccess()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.SourcePoint_1000Balance,
                _schema.RecipientPoint_AllRest1,
                _schema.RecipientPoint_AllRest2,
                _schema.RecipientPoint_AllRest3,
                _schema.RecipientPoint_AllRest4,
                _schema.RecipientPoint_AllRest5,
                _schema.RecipientPoint_AllRest6,
                _schema.RecipientPoint_AllRest7,
            });
        }
    }
}