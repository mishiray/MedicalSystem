using AutoMapper;
using MedicalSystem.Controllers;
using MedicalSystem.DTOs;
using MedicalSystem.DTOs.ControllerDtos;
using MedicalSystem.DTOs.Enums;
using MedicalSystem.Entities;
using MedicalSystem.Services;
using MedicalSystem.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;

namespace MedicalSystem.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            this.mapper = mapper;
            this.userService = userService;
        }

        [HttpPost("get-logged-in-user")]
        [ProducesResponseType(typeof(GlobalResponse<GetUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CancellationToken token)
        {
            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var result = await userService.GetUserById(loggedInUser, token);

            switch (result.Response)
            {
                case ServiceResponses.BadRequest:
                    ModelState.AddModelError($"{result.Response}", result.Message);
                    return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));

                case ServiceResponses.NotFound:
                    ModelState.AddModelError($"{result.Response}", result.Message);
                    return NotFound(ResponseBuilder.BuildResponse<object>(ModelState, null));

                case ServiceResponses.Failed:
                    ModelState.AddModelError($"{result.Response}", result.Message);
                    return UnprocessableEntity(ResponseBuilder.BuildResponse<object>(ModelState, null));

                case ServiceResponses.Success:
                    return Ok(ResponseBuilder.BuildResponse<object>(null, mapper.Map<GetUserDto>(result.Data)));

                default:
                    ModelState.AddModelError($"{result.Response}", result.Message);
                    return UnprocessableEntity(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }
        }

    }
}
