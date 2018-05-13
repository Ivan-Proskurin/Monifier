using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class ProductCommands : IProductCommands
    {
        private readonly IEntityRepository _repository;
        private readonly ICurrentSession _currentSession;

        public ProductCommands(IEntityRepository repository, ICurrentSession currentSession)
        {
            _repository = repository;
            _currentSession = currentSession;
        }

        public async Task<ProductModel> Update(ProductModel model)
        {
            var product = await _repository.LoadAsync<Product>(model.Id);
            if (product == null)
                throw new ArgumentException($"Нет продукта с идентификатором {model.Id}");
            product.Name = model.Name;
            product.CategoryId = model.CategoryId;
            _repository.Update(product);
            await _repository.SaveChangesAsync().ConfigureAwait(false);
            return model;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            var product = await _repository.LoadAsync<Product>(id);
            if (product == null)
                throw new ArgumentException($"Нет товара с идентификтором Id = {id}");

            if (onlyMark)
            {
                product.IsDeleted = true;
                _repository.Update(product);
            }
            else
            {
                // todo: удалить все операции по продукту
                _repository.Delete(product);
            }
            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<ProductModel> AddProductToCategory(int categoryId, string productName)
        {
            var model = new Product
            {
                CategoryId = categoryId,
                Name = productName,
                OwnerId = _currentSession.UserId
            };
            _repository.Create(model);
            await _repository.SaveChangesAsync().ConfigureAwait(false);
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
            foreach (var id in ids)
            {
                var model = await _repository.LoadAsync<Product>(id).ConfigureAwait(false);
                if (model == null) continue;
                if (onlyMark)
                {
                    model.IsDeleted = true;
                    _repository.Update(model);
                }
                else
                {
                    // todo: удалить все операции по продукту
                    _repository.Delete(model);
                }
                deletedList.Add(id);
            }
            await _repository.SaveChangesAsync().ConfigureAwait(false);
            return deletedList;
        }
    }
}