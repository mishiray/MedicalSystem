using MedicalSystem.Controllers;
using MedicalSystem.DTOs;
using MedicalSystem.DTOs.Enums;
using MedicalSystem.Services;
using MedicalSystem.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Controllers.V1
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUserService userService;

        public AuthenticationController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(GlobalResponse<AuthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequestModel model)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            return new ControllerResponse().ReturnResponse(await userService.Login(model));
        }

    }
}
