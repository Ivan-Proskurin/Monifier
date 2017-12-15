using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Monifier.Web.Extensions
{
    public static class PageModelExtensions
    {
        public static async Task<JsonResult> ProcessAjaxPostRequestAsync(
            this PageModel page, Func<string, Task<object>> processRequestAsync)
        {
            using (var stream = new MemoryStream())
            {
                page.Request.Body.CopyTo(stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    var requestBody = reader.ReadToEnd();
                    var result = await processRequestAsync(requestBody);
                    return new JsonResult(result);
                }
            }
        }
    }
}