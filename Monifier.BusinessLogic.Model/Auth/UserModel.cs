using Monifier.DataAccess.Model.Auth;

namespace Monifier.BusinessLogic.Model.Auth
{
    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Login { get; set; }
        public string Hash { get; set; }
        public string Salt { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; }
    }

    public static class UserModelExtensions
    {
        public static User ToUser(this UserModel model)
        {
            return new User
            {
                Id = model.Id,
                Name = model.Name,
                Login = model.Name,
                Hash = model.Hash,
                Salt = model.Salt,
                IsAdmin = model.IsAdmin,
                IsDeleted = model.IsDeleted
            };
        }
    }
}