
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
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
        IGoogleRecaptchaV3Service _gService { get; set; }
        public SignUp(IGoogleRecaptchaV3Service gService)
        {
            _gService = gService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromQuery] SignUpModel SignUpData)
        {
            GRequestModel rm = new GRequestModel(SignUpData.RecaptchaToken,
                                                 HttpContext.Connection.RemoteIpAddress.ToString());

            _gService.InitializeRequest(rm);

            if (!await _gService.Execute())
            {
                //return error codes string.
                return Ok(_gService.Response.error_codes);
            }

            //call Business layer

            //return result
            //TODO: possibly return de-serialized Response from Google API.
            return Ok("Server-side Google reCaptcha validation successfull!");
        }
    }
}