using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeAllCore.Models;
using BeAllCore;
using BeAllCore.Objects;
using System.Threading;
using BeAllCore.Forms;
using System.Text.RegularExpressions;

namespace BeAllCore.Controllers
{
    [Route("/[controller]")]
    public class centralController : Controller
    {
        // POST /Central/postLog
        [HttpPost("/[controller]/postLog", Name = nameof(postLog))]
        public async Task<IActionResult> postLog([FromBody]PostLogForm InputForm, CancellationToken ct)
        {
            //validate
            if (InputForm.token == null)
            {
                return ApiResponse.Error(Request, "token missing", "function must input token", ApiError.Err_MissingParameter);
            }

            if (InputForm.token != "1234567890")
            {
                return ApiResponse.Error(Request, "token wrong", "", ApiError.Err_MissingParameter);
            }

            await DataBaseTest.InsertNewCentralLog(InputForm.identifiers, InputForm.status, InputForm.token);

            //6. success
            return ApiResponse.Success(Request);
        }

    }
}
