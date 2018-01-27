using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Monifier.BusinessLogic.Contract.Inventorization;
using Monifier.BusinessLogic.Model.Inventorization;

namespace Monifier.Web.Api.Inventorization
{
    [Route("api/inventorization")]
    public class InventorizationController : Controller
    {
        private readonly IInventorizationQueries _inventorizationQueries;

        public InventorizationController(IInventorizationQueries inventorizationQueries)
        {
            _inventorizationQueries = inventorizationQueries;
        }
        
        [HttpGet("state")]
        public async Task<BalanceState> GetState()
        {
            return await _inventorizationQueries.GetBalanceState();
        }
    }
}