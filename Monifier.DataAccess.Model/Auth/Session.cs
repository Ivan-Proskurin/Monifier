using System;
using System.ComponentModel.DataAnnotations.Schema;
using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Auth
{
    public class Session : IHasId
    {
        public int Id { get; set; }
        
        public Guid Token { get; set; }
        
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User User { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime? Expiration { get; set; }
        
        public bool IsAdmin { get; set; }
    }
}