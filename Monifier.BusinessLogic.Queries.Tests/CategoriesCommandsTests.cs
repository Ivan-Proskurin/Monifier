using System;
using System.Threading.Tasks;
using Monifier.BusinessLogic.Contract.Auth;
using Monifier.BusinessLogic.Contract.Base;
using Monifier.BusinessLogic.Model.Base;
using Monifier.BusinessLogic.Queries.Base;
using Monifier.DataAccess.Contract;
using Monifier.DataAccess.EntityFramework;
using Monifier.DataAccess.Model.Base;
using Moq;
using Xunit;

namespace Monifier.BusinessLogic.Queries.Tests
{
    public class CategoriesCommandsTests
    {
        private readonly Mock<INamedModelQueryRepository<Category>> _queriesMock;
        private readonly Mock<ICommandRepository<Category>> _commandsMock;
        private readonly Mock<IProductQueries> _productQueriesMock;
        private readonly Mock<IProductCommands> _productCommandsMock;
        private readonly Mock<IUnitOfWork> _uowMock;
        private readonly Mock<ICurrentSession> _currentSessionMock;
        private readonly IEntityRepository _repository;

        public CategoriesCommandsTests()
        {
            _queriesMock = new Mock<INamedModelQueryRepository<Category>>();
            _queriesMock.Setup(m => m.GetByName(It.IsAny<int>(), It.IsAny<string>()))
                .Returns<int, string>((id, s) => Task.FromResult<Category>(null));
            _commandsMock = new Mock<ICommandRepository<Category>>();
            _commandsMock.Setup(m => m.Create(It.IsAny<Category>()));
            _productQueriesMock = new Mock<IProductQueries>();
            _productCommandsMock = new Mock<IProductCommands>();
            _currentSessionMock = new Mock<ICurrentSession>();
            _currentSessionMock.Setup(m => m.UserId).Returns(1);
            _uowMock = new Mock<IUnitOfWork>();
            _uowMock.Setup(m => m.GetQueryRepository<Category>()).Returns(_queriesMock.Object);
            _uowMock.Setup(m => m.GetCommandRepository<Category>()).Returns(_commandsMock.Object);
            _uowMock.Setup(m => m.GetNamedModelQueryRepository<Category>()).Returns(_queriesMock.Object);
            _uowMock.Setup(m => m.SaveChangesAsync()).Returns(Task.Run(() => { }));
            _repository = new EntityRepository(_uowMock.Object);
        }

        [Fact]
        public void UpdateWithUniqueName_UpdatesNormal()
        {

            var categoriesCommands = new CategoriesCommands(
                _repository, _productCommandsMock.Object, _productQueriesMock.Object, _currentSessionMock.Object);
            var model = new CategoryModel { Id = -1, Name = "New category" };
            var t = categoriesCommands.Update(model);
            t.Wait();

            _commandsMock.Verify(m => m.Create(It.IsAny<Category>()));
            _uowMock.Verify(m => m.SaveChangesAsync());
        }

        [Fact]
        public async Task UpdateWithNonUniqueName_ThrowsArgumentException()
        {
            _queriesMock.Setup(m => m.GetByName(It.IsAny<int>(), It.IsAny<string>())).Returns<int, string>(
                (id, s) => Task.FromResult(new Category { Id = 1, Name = "New category"}));

            var categoriesCommands = new CategoriesCommands(
                _repository, _productCommandsMock.Object, _productQueriesMock.Object, _currentSessionMock.Object);
            var model = new CategoryModel { Name = "New category" };
            await Assert.ThrowsAsync<ArgumentException>(async () => await categoriesCommands.Update(model));
        }
    }
}
