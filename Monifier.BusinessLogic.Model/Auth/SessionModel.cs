using System;

namespace Monifier.BusinessLogic.Model.Auth
{
    public class SessionModel
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Expiration { get; set; }
    }
}