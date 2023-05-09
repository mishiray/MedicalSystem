using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MedicalSystem.Entities;
using MedicalSystem.Utilities;
using System.Data;

namespace MedicalSystem.Controllers.V1
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RolesController : ControllerBase
    {
        public RoleManager<IdentityRole> RoleManager { get; }

        public RolesController(RoleManager<IdentityRole> roleManager)
        {
            RoleManager = roleManager;
        }


        [HttpPost("create")]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> CreateRole(string rolename)
        {
            if (string.IsNullOrEmpty(rolename))
            {
                ModelState.AddModelError("BadRequest", "Role name cannot be empty");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            if (await RoleManager.FindByNameAsync(rolename) is not null)
            {
                ModelState.AddModelError("BadRequest", "Role already exists");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            var result = await RoleManager.CreateAsync(new IdentityRole(rolename));
            if (result.Succeeded)
            {
                return Ok(ResponseBuilder.BuildResponse<object>(null, "Role created successfully"));
            }

            ModelState.AddModelError("BadRequest", "An error occured");
            return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
        }


        [HttpGet("list-all")]
        [AllowAnonymous]
        public async Task<IActionResult> ListAll(CancellationToken token)
        {
            var roles = await RoleManager.Roles.ToListAsync(token);
            return Ok(ResponseBuilder.BuildResponse<object>(null, roles));
        }
    }
}
