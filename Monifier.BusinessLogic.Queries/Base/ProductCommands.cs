using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class ProductCommands : IProductCommands
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductCommands(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductModel> Update(ProductModel model)
        {
            var commands = _unitOfWork.GetCommandRepository<Product>();
            var queries = _unitOfWork.GetQueryRepository<Product>();
            var product = await queries.GetById(model.Id);
            if (product == null)
                throw new ArgumentException($"Нет продукта с идентификатором {model.Id}");
            product.Name = model.Name;
            product.CategoryId = model.CategoryId;
            commands.Update(product);
            await _unitOfWork.SaveChangesAsync();
            return model;
        }

        public Task Delete(int id, bool onlyMark = true)
        {
            throw new NotImplementedException();
        }

        public async Task<ProductModel> AddProductToCategory(int categoryId, string productName)
        {
            var commands = _unitOfWork.GetCommandRepository<Product>();
            var model = new Product
            {
                Id = -1,
                CategoryId = categoryId,
                Name = productName
            };
            commands.Create(model);
            await _unitOfWork.SaveChangesAsync();
            return new ProductModel
            {
                Id = model.Id,
                CategoryId = model.CategoryId,
                Name = model.Name
            };
        }

        public async Task<List<int>> GroupDeletion(int[] ids, bool onlyMark = true)
        {
            var deletedList = new List<int>();
            var queries = _unitOfWork.GetQueryRepository<Product>();
            var commands = _unitOfWork.GetCommandRepository<Product>();
            foreach (var id in ids)
            {
                var model = await queries.GetById(id);
                if (model == null) continue;
                if (onlyMark)
                {
                    model.IsDeleted = true;
                    commands.Update(model);
                }
                else
                {
                    // todo: удалить все операции по продукту
                    commands.Delete(model);
                }
                deletedList.Add(id);
            }
            await _unitOfWork.SaveChangesAsync();
            return deletedList;
        }
    }
}