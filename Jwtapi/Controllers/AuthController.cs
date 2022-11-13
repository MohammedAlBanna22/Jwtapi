using System.Threading.Tasks;
using Jwtapi.Models.DTO;
using Jwtapi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jwtapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authservice;
        public AuthController(IAuthService authservice)
        {
            _authservice = authservice;
        }
        // iportant to put ("rgister") to use many httpPost and dosenot reflect
        [HttpPost("register")]
        public async Task<ActionResult> RegisgterAsync([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result= await _authservice.RegisterAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            return Ok(result);

            //when you want to return token and expiration of token parmeter as below
            //return Ok(new { token=result.Token,expireson=result.ExpiresOn});



        }

        [HttpPost("token")]
        public async Task<ActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authservice.GetTokenAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result.Message);
            return Ok(result);


        }

        //when you want to make it after authntication 
        //use token in request in postman in auhtntication beside parmeter like end vidioe of devcourse
        // [Authorize(Roles ="admin")]
       // [Authorize]
        [HttpPost("addrole")]
        public async Task<ActionResult> AddRoleModel([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _authservice.AddRoleAsync(model);
            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);
            return Ok(model);


        }


    }
}
