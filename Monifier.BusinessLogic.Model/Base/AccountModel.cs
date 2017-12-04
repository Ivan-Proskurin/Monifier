using System;

namespace Monifier.BusinessLogic.Model.Base
{
    public class AccountModel
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public DateTime DateCreated { get; set; }
        public string Name { get; set; }
        public decimal Balance { get; set; }
    }
}