namespace Monifier.BusinessLogic.Distribution.Model
{
    public class DistributionSource
    {
        public int Id { get; set; }
        public bool CanFlow { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public string WithdrawTotal { get; set; }
        public string Result { get; set; }
    }
}