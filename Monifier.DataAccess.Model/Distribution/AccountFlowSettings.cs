using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Distribution
{
    public class AccountFlowSettings : IHasId
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        [ForeignKey("AccountId")]
        public Account Account { get; set; }
        public bool CanFlow { get; set; }
    }
}