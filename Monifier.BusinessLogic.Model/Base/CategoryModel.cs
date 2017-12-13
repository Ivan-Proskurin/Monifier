using Monifier.DataAccess.Model.Contracts;

namespace Monifier.BusinessLogic.Model.Base
{
    public class CategoryModel : IHasName
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProductCount { get; set; }
    }
}
