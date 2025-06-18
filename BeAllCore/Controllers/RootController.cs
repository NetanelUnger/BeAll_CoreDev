using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BeAllCore.Models;

namespace BeAllCore.Controllers
{
    [Route("/")]
    public class Users: Controller
    {
        [HttpGet]
        public IActionResult Test()
        {
            return ApiResponse.Success(Request, "Server is runing");
        }
    }
}
