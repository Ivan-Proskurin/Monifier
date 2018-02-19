using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Monifier.BusinessLogic.Contract.Distribution;
using Monifier.BusinessLogic.Distribution;
using Monifier.BusinessLogic.Distribution.Model;
using Monifier.Web.Api.Models;
using Monifier.Web.Extensions;

namespace Monifier.Web.Pages.Distribution
{
    [Authorize]
    public class DistributionModel : PageModel
    {
        private readonly IDistributionQueries _distributionQueries;
        private readonly IDistributionCommands _distributionCommands;

        public DistributionModel(IDistributionQueries distributionQueries,
            IDistributionCommands distributionCommands)
        {
            _distributionQueries = distributionQueries;
            _distributionCommands = distributionCommands;
        }
        
        public DistributionBoard Board { get; private set; }

        public async Task OnGetAsync()
        {
            Board = await _distributionQueries.GetDistributionBoard();
        }

        public async Task<JsonResult> OnPostDistributeAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async request =>
            {
                try
                {
                    var board = request.FromJson<DistributionBoard>();
                    await _distributionCommands.Distribute(board);
                    return AjaxResponse.SuccessResponse(board.ToJson());
                }
                catch (FlowDistributionException exc)
                {
                    return AjaxResponse.ErrorResponse(exc.Message);
                }
                catch (Exception exc)
                {
                    return AjaxResponse.ErrorResponse(exc.ToString());
                }
            });
        }

        public async Task<JsonResult> OnPostSaveAsync()
        {
            return await this.ProcessAjaxPostRequestAsync(async request =>
            {
                try
                {
                    var board = request.FromJson<DistributionBoard>();
                    await _distributionCommands.Save(board);
                    return AjaxResponse.SuccessResponse();
                }
                catch (Exception exc)
                {
                    return AjaxResponse.ErrorResponse(exc.ToString());
                }
            });
        }
    }
}