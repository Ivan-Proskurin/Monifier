namespace Monifier.Web.Api.Models
{
    public class AjaxResponse
    {      
        public bool Success { get; set; }
        public string Error { get; set; }
        public string Result { get; set; }

        public static AjaxResponse SuccessResponse()
        {
            return new AjaxResponse
            {
                Success = true,
                Result = string.Empty,
                Error = string.Empty,
            };
        }

        public static AjaxResponse SuccessResponse(string result)
        {
            return new AjaxResponse
            {
                Success = true,
                Result = result,
                Error = string.Empty,
            };
        }

        public static AjaxResponse ErrorResponse(string error)
        {
            return new AjaxResponse
            {
                Success = false,
                Result = string.Empty,
                Error = error
            };
        }
    }
}