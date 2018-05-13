using System;
using System.Linq;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.Model.Base;
using Monifier.DataAccess.Model.Expenses;

namespace Monifier.BusinessLogic.Queries.Base
{
    public class CategoriesCommands : ICategoriesCommands
    {
        private readonly IEntityRepository _repository;
        private readonly IProductCommands _productCommands;
        private readonly IProductQueries _productQueries;
        private readonly ICurrentSession _currentSession;

        public CategoriesCommands(IEntityRepository repository, 
            IProductCommands productCommands, 
            IProductQueries productQueries,
            ICurrentSession currentSession)
        {
            _repository = repository;
            _productCommands = productCommands;
            _productQueries = productQueries;
            _currentSession = currentSession;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            var category = await _repository.LoadAsync<Category>(id);
            if (category == null)
                throw new ArgumentException($"Нет категории с идентификатором {id}");
            var products = await _productQueries.GetCategoryProducts(category.Id, true);
            if (onlyMark)
            {
                await _productCommands.GroupDeletion(products.Select(x => x.Id).ToArray());
                category.IsDeleted = true;
                _repository.Update(category);
            }
            else
            {
                await _productCommands.GroupDeletion(products.Select(x => x.Id).ToArray(), false);
                _repository.Delete(category);
            }
            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<CategoryModel> Update(CategoryModel model)
        {
            var category = new Category
            {
                Name = model.Name,
                OwnerId = _currentSession.UserId
            };
            var other = await _repository.FindByNameAsync<Category>(_currentSession.UserId, model.Name);
            if (other != null)
            {
                if (other.Id != category.Id)
                    throw new ArgumentException("Категория товаров с таким именем уже существует");
                else
                    _repository.Detach(other);
            }

            if (model.Id > 0)
            {
                category.Id = model.Id;
                _repository.Update(category);
            }
            else
            {
                _repository.Create(category);
            }
            
            await _repository.SaveChangesAsync().ConfigureAwait(false);

            model.Id = category.Id;
            return model;
        }

        public async Task<CategoryModel> CreateNewOrBind(int flowId, string categoryName)
        {
            var category = await _repository.FindByNameAsync<Category>(_currentSession.UserId, categoryName);
            if (category == null)
            {
                category = new Category
                {
                    Name = categoryName,
                    OwnerId = _currentSession.UserId
                };
                _repository.Create(category);
                await _repository.SaveChangesAsync().ConfigureAwait(false);
            }

            _repository.Create(new ExpensesFlowProductCategory
            {
                CategoryId = category.Id,
                ExpensesFlowId = flowId
            });
            await _repository.SaveChangesAsync().ConfigureAwait(false);
            
            return new CategoryModel
            {
                Id = category.Id,
                Name = category.Name
            };
        }
        
    }
}
