using Monifier.DataAccess.Model.Contracts;

namespace Monifier.DataAccess.Model.Auth
{
    public class User : IHasId, IHasName
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }
    }
}