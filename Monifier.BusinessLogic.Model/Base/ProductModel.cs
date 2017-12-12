using Monifier.DataAccess.Contract.Model;

namespace Monifier.BusinessLogic.Model.Base
{
    public class ProductModel : IHasName
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }
}
