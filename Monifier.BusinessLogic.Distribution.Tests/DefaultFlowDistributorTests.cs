using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Monifier.BusinessLogic.Distribution.Tests.Models;
using Monifier.BusinessLogic.Model.Distribution;

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
                _schema.RecipientPoint_FixedFromRest500
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
                    _schema.RecipientPoint_FixedFromRest500,
                    _schema.RecipientPoint_FixedFromRest3000,
                    _schema.RecipientPoint_FixedFromBase1000,
                    _schema.RecipientPoint_PercentFromBase25,
                });
                Assert.Fail("Ожидался выброс FlowDistributionException");
            }
            catch (FlowDistributionException exc)
            {
                Assert.AreEqual(1, exc.Sources.Count);
                Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, exc.Sources[0].Name);
                Assert.IsNotNull(exc.Recipient);
                Assert.AreEqual(_schema.RecipientPoint_FixedFromRest500.Name, exc.Recipient.Name);
            }
            
            Assert.AreEqual(0, target.FlowRecords.Count);
            Assert.AreEqual(5000, _schema.SourcePoint_5000Balance.Balance);
            Assert.AreEqual(0, _schema.SourcePoint_ZeroBalance.Balance);
            Assert.AreEqual(10, _schema.RecipientPoint_FixedFromRest500.Balance);
            Assert.AreEqual(100, _schema.RecipientPoint_FixedFromRest3000.Balance);
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
                _schema.RecipientPoint_FixedFromRest3000,
                _schema.RecipientPoint_FixedFromBase1000,
                _schema.RecipientPoint_PercentFromBase25,
            });
            
            Assert.AreEqual(3, target.FlowRecords.Count);
            var flow1 = target.FlowRecords[0];
            var flow2 = target.FlowRecords[1];
            var flow3 = target.FlowRecords[2];
            
            Assert.AreEqual(0, _schema.SourcePoint_5000Balance.Balance);
            
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow1.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase1000.Name, flow1.Recipient.Name);
            Assert.AreEqual(1000, flow1.Amount);
            Assert.AreEqual(1300, _schema.RecipientPoint_FixedFromBase1000.Balance);
            
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow2.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_PercentFromBase25.Name, flow2.Recipient.Name);
            Assert.AreEqual(1000, flow2.Amount);
            Assert.AreEqual(1700, _schema.RecipientPoint_PercentFromBase25.Balance);

            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow3.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromRest3000.Name, flow3.Recipient.Name);
            Assert.AreEqual(3000, flow3.Amount);
            Assert.AreEqual(3100, _schema.RecipientPoint_FixedFromRest3000.Balance);
        }

        [TestMethod]
        public void MultipleSourcesBalanceDivision_DistributionSuccess()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.SourcePoint_5000Balance,
                _schema.SourcePoint_1000Balance,
                _schema.RecipientPoint_FixedFromRest3000,
                _schema.RecipientPoint_FixedFromRest2500
            });
            
            Assert.AreEqual(3, target.FlowRecords.Count);
            var flow1 = target.FlowRecords[0];
            var flow2 = target.FlowRecords[1];
            var flow3 = target.FlowRecords[2];
            
            Assert.AreEqual(0, _schema.SourcePoint_5000Balance.Balance);
            Assert.AreEqual(500, _schema.SourcePoint_1000Balance.Balance);
            
            // flow1
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow1.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromRest3000.Name, flow1.Recipient.Name);
            Assert.AreEqual(3000, flow1.Amount);
            Assert.AreEqual(3100, _schema.RecipientPoint_FixedFromRest3000.Balance);
            
            // flow2
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow2.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromRest2500.Name, flow2.Recipient.Name);
            Assert.AreEqual(2000, flow2.Amount);

            // flow3
            Assert.AreEqual(_schema.SourcePoint_1000Balance.Name, flow3.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromRest2500.Name, flow3.Recipient.Name);
            Assert.AreEqual(500, flow3.Amount);
            Assert.AreEqual(3400, _schema.RecipientPoint_FixedFromRest2500.Balance);
        }

        // Тестовый сценарий Case1.odt
        [TestMethod]
        public void ComplexDistribution_DistributionSucess()
        {
            var target = new DefaultFlowDistributor();
            
            target.Distribute(new []
            {
                _schema.SourcePoint_ZeroBalance,
                _schema.NotParticipantSourcePoint,
                _schema.SourcePoint_5000Balance,
                _schema.SourcePoint_1000Balance,
                _schema.RecipientPoint_FixedFromBase1000, // 300
                _schema.SourcePoint_Minus1000BothDestination, // fixed from base 2000
                _schema.RecipientPoint_PercentFromRest30, // 450
                _schema.RecipientPoint_PercentFromBase25, // 700
                _schema.RecipientPoint_AllRest1, // 0
                _schema.RecipientPoint_AllRest2, // 100
                _schema.NotParticipantRecipientPoint,
                _schema.RecipientPoint_FixedFromRest2500,
            });
            
            Assert.AreEqual(9, target.FlowRecords.Count);
            var flow1 = target.FlowRecords[0];
            var flow2 = target.FlowRecords[1];
            var flow3 = target.FlowRecords[2];
            var flow4 = target.FlowRecords[3];
            var flow5 = target.FlowRecords[4];
            var flow6 = target.FlowRecords[5];
            var flow7 = target.FlowRecords[6];
            var flow8 = target.FlowRecords[7];
            var flow9 = target.FlowRecords[8];
            
            Assert.AreEqual(0, _schema.SourcePoint_5000Balance.Balance);
            Assert.AreEqual(0, _schema.SourcePoint_1000Balance.Balance);
            Assert.AreEqual(0, _schema.SourcePoint_Minus1000BothDestination.Balance);
            
            // flow1
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow1.Source.Name);
            Assert.AreEqual(_schema.SourcePoint_Minus1000BothDestination.Name, flow1.Recipient.Name);
            Assert.AreEqual(2000, flow1.Amount);
            
            // flow2
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow2.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromBase1000.Name, flow2.Recipient.Name);
            Assert.AreEqual(1000, flow2.Amount);
            Assert.AreEqual(1300, _schema.RecipientPoint_FixedFromBase1000.Balance);

            // flow3
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow3.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_PercentFromBase25.Name, flow3.Recipient.Name);
            Assert.AreEqual(1000, flow3.Amount);
            Assert.AreEqual(1700, _schema.RecipientPoint_PercentFromBase25.Balance);
            
            // flow4
            Assert.AreEqual(_schema.SourcePoint_5000Balance.Name, flow4.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromRest2500.Name, flow4.Recipient.Name);
            Assert.AreEqual(1000, flow4.Amount);
            
            // flow5
            Assert.AreEqual(_schema.SourcePoint_1000Balance.Name, flow5.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromRest2500.Name, flow5.Recipient.Name);
            Assert.AreEqual(1000, flow5.Amount);
            
            // flow6
            Assert.AreEqual(_schema.SourcePoint_Minus1000BothDestination.Name, flow6.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_FixedFromRest2500.Name, flow6.Recipient.Name);
            Assert.AreEqual(500, flow6.Amount);
            Assert.AreEqual(3400, _schema.RecipientPoint_FixedFromRest2500.Balance);
            
            // flow7
            Assert.AreEqual(_schema.SourcePoint_Minus1000BothDestination.Name, flow7.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_PercentFromRest30.Name, flow7.Recipient.Name);
            Assert.AreEqual(150, flow7.Amount);
            Assert.AreEqual(600, _schema.RecipientPoint_PercentFromRest30.Balance);
            
            // flow8
            Assert.AreEqual(_schema.SourcePoint_Minus1000BothDestination.Name, flow8.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_AllRest1.Name, flow8.Recipient.Name);
            Assert.AreEqual(175, flow8.Amount);
            Assert.AreEqual(175, _schema.RecipientPoint_AllRest1.Balance);
            
            // flow9
            Assert.AreEqual(_schema.SourcePoint_Minus1000BothDestination.Name, flow9.Source.Name);
            Assert.AreEqual(_schema.RecipientPoint_AllRest2.Name, flow9.Recipient.Name);
            Assert.AreEqual(175, flow9.Amount);
            Assert.AreEqual(275, _schema.RecipientPoint_AllRest2.Balance);
        }
    }
}