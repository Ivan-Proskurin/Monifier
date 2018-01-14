using Monifier.DataAccess.Model.Distribution;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Monifier.BusinessLogic.Distribution.Model
{
    public class DistributionRecipient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
        public bool CanFlow { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public FlowRule Rule { get; set; }
        public decimal Amount { get; set; }        
        public string TopupTotal { get; set; }
        public string Result { get; set; }
    }
}