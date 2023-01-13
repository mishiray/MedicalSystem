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

        [HttpGet("get-logged-in-user")]
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

        [HttpPatch("{userId}/update-roles")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(GlobalResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> EditRoles([Required] string userId, UpdateRolesDto model, CancellationToken token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            var user = await userService.ListAll().FirstOrDefaultAsync(c => c.Id == userId, token);

            if (user is null)
            {
                ModelState.AddModelError("UserNotFound", $"User not found");
                return NotFound(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            if (model.Roles.Count < 1)
            {
                ModelState.AddModelError("BadRequest", "You must add at least one role");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            if (model.Roles.Count > 0)
            {
                foreach (string role in model.Roles)
                {
                    var roleExists = await userService.RoleManager.Roles.FirstOrDefaultAsync(c => c.Name == role) != null;
                    if (roleExists == false)
                    {
                        ModelState.AddModelError("BadRequest", $"Role {role} does not exist");
                        return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
                    }
                }
            }

            var getUserRoles = await userService.UserManager.GetRolesAsync(user);
            var removeUserRoles = await userService.UserManager.RemoveFromRolesAsync(user, getUserRoles);
            if (!removeUserRoles.Succeeded)
            {
                ModelState.AddModelError("UnprocessableEntity", $"Error while changing role");
                return UnprocessableEntity(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            var addRoles = await userService.UserManager.AddToRolesAsync(user, model.Roles);
            if (!addRoles.Succeeded)
            {
                ModelState.AddModelError("BadRequest", "Unable To Change User Role");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            user.Roles = model.Roles;
            await userService.UpdateUser(user);

            return Ok(ResponseBuilder.BuildResponse<object>(null, "Roles Updated Successfully"));
        }


    }
}
