using System.ComponentModel;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;


namespace BeAllCore.Models
{
    public static class ApiResponse
    {
        public static IActionResult Success(Microsoft.AspNetCore.Http.HttpRequest request, object results)
        {
            ObjectResult a = new ObjectResult(results);
            a.StatusCode = 200;

            request.HttpContext.Response.Headers.Add("result-code", "0");

            return a;
        }

        public static IActionResult Success(Microsoft.AspNetCore.Http.HttpRequest request, string message)
        {
            var results = new
            {
                message = message,
                status = "success"
            };

            ObjectResult a = new ObjectResult(results);
            a.StatusCode = 200;

            request.HttpContext.Response.Headers.Add("result-code", "0");

            return a;
        }

        public static IActionResult Success(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            var results = new
            {
                status = "success"
            };

            ObjectResult a = new ObjectResult(results);
            a.StatusCode = 200;

            request.HttpContext.Response.Headers.Add("result-code", "0");

            return a;
        }

        public static ObjectResult Error(Microsoft.AspNetCore.Http.HttpRequest request, string Message, string Detail, int code)
        {
            ApiError e = new ApiError();
            e.Message = Message;
            e.Detail = Detail;
            e.Code = code;
            e.Status = "error";

            ObjectResult a = new ObjectResult(e);
            a.StatusCode = 200;

            request.HttpContext.Response.Headers.Add("result-code", e.Code.ToString());
            try {
                DataBase.InsertLog(Message, Detail);
            }
            catch
            {

            }

            return a;
        }
    }
    public class ApiError
    {
        public const int Err_MissingParameter = 100;
        public const int Err_UserNotFound = 101;
        public const int Err_CantSendSMS = 102;
        public const int Err_MongoError = 103;
        public const int Err_SmsSentLessThen15MinAgo = 104;
        public const int Err_NumberFormatIncorrect = 106;
        public const int Err_GateIdentifierNotFound = 107;
        public const int Err_UserDontHaveAccess = 108;

        public const int Err_WrongVerifyCode = 105;

        public const int Err_NoToken = 106;
        public const int Err_UserToAddNotListed = 109;
        public const int Err_UserAlreadyHaveAccess = 110;

        public const int Err_UserAlreadySign = 111;

        public const int Err_Unknown = 112;

        public string Message { get; set; }
        public string Detail { get; set; }

        public string Status { get; set; }

        public int Code { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore) ]
        [DefaultValue("")]
        public string stackTrace { get; set; }

    }
}
