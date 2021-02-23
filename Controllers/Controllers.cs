
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using AspNetCoreRecaptchaV3ValidationDemo.Tooling;

namespace AspNetCoreRecaptchaV3ValidationDemo.Controllers
{
   public class SignUpModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RecaptchaToken { get; set; }
    }

    [ApiController]
    [Route("public/signup")]
    public class SignUp : ControllerBase
    {
        GoogleRecaptchaV3Service _gService { get; set; }
        public SignUp(GoogleRecaptchaV3Service gService)
        {
            _gService = gService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SignUpModel SignUpData)
        {
            GRequestModel rm = new GRequestModel(SignUpData.RecaptchaToken,
                                                 HttpContext.Connection.RemoteIpAddress.ToString());
            
            _gService.InitializeRequest(rm);

            if(!await _gService.Execute())
            {
                return StatusCode(500);
            }
         
            //call Business layer

            //return result
            return Ok("You are really really not a robot");
        }
    } 
}