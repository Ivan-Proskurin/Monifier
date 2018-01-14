using System;
using System.Collections.Generic;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Distribution
{
    public class Distribution : IHasId
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public decimal SumFlow { get; set; }
        public virtual ICollection<Flow> Flows { get; set; }
    }
}