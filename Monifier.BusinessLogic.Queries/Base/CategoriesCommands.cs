﻿using System;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductCommands _productCommands;
        private readonly IProductQueries _productQueries;
        private readonly ICurrentSession _currentSession;

        public CategoriesCommands(IUnitOfWork unitOfWork, 
            IProductCommands productCommands, 
            IProductQueries productQueries,
            ICurrentSession currentSession)
        {
            _unitOfWork = unitOfWork;
            _productCommands = productCommands;
            _productQueries = productQueries;
            _currentSession = currentSession;
        }

        public async Task Delete(int id, bool onlyMark = true)
        {
            var categoriesCommands = _unitOfWork.GetCommandRepository<Category>();
            var categoriesQueries = _unitOfWork.GetQueryRepository<Category>();
            var category = await categoriesQueries.GetById(id);
            if (category == null)
                throw new ArgumentException($"Нет категории с идентификатором {id}");
            var products = await _productQueries.GetCategoryProducts(category.Id, true);
            if (onlyMark)
            {
                await _productCommands.GroupDeletion(products.Select(x => x.Id).ToArray());
                category.IsDeleted = true;
                categoriesCommands.Update(category);
            }
            else
            {
                await _productCommands.GroupDeletion(products.Select(x => x.Id).ToArray(), false);
                categoriesCommands.Delete(category);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<CategoryModel> Update(CategoryModel model)
        {
            var categoryCommands = _unitOfWork.GetCommandRepository<Category>();
            var category = new Category
            {
                Name = model.Name,
                OwnerId = _currentSession.UserId
            };
            var categoriesQueries = _unitOfWork.GetNamedModelQueryRepository<Category>();
            var other = await categoriesQueries.GetByName(_currentSession.UserId, model.Name);
            if (other != null)
            {
                if (other.Id != category.Id)
                    throw new ArgumentException("Категория товаров с таким именем уже существует");
                else
                    categoryCommands.Detach(other);
            }

            if (model.Id > 0)
            {
                category.Id = model.Id;
                categoryCommands.Update(category);
            }
            else
            {
                categoryCommands.Create(category);
            }
            
            await _unitOfWork.SaveChangesAsync();

            model.Id = category.Id;
            return model;
        }

        public async Task<CategoryModel> CreateNewOrBind(int flowId, string categoryName)
        {
            var categoryQueries = _unitOfWork.GetNamedModelQueryRepository<Category>();
            var categoryCommands = _unitOfWork.GetCommandRepository<Category>();
            
            var category = await categoryQueries.GetByName(_currentSession.UserId, categoryName);
            if (category == null)
            {
                category = new Category
                {
                    Name = categoryName,
                    OwnerId = _currentSession.UserId
                };
                categoryCommands.Create(category);
                await _unitOfWork.SaveChangesAsync();
            }

            var catFlowsCommands = _unitOfWork.GetCommandRepository<ExpensesFlowProductCategory>();
            catFlowsCommands.Create(new ExpensesFlowProductCategory
            {
                CategoryId = category.Id,
                ExpensesFlowId = flowId
            });
            await _unitOfWork.SaveChangesAsync();
            
            return new CategoryModel
            {
                Id = category.Id,
                Name = category.Name
            };
        }
        
    }
}
