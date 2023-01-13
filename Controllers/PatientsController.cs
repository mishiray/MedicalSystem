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
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService patientService;
        private readonly IRecordService recordService;
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public PatientsController(IPatientService patientService, IMapper mapper, IRecordService recordService, IUserService userService)
        {
            this.patientService = patientService;
            this.mapper = mapper;
            this.recordService = recordService;
            this.userService = userService;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(GlobalResponse<GetUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Role2")]
        public async Task<IActionResult> Create(CreateUserDto model, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));

            var user = mapper.Map<Patient>(model);

            var result = await patientService.Create(user, token);

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


                    return Ok(ResponseBuilder.BuildResponse<object>(null, mapper.Map<GetUserDto>(result.Data.User)));

                default:
                    ModelState.AddModelError($"{result.Response}", result.Message);
                    return UnprocessableEntity(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }
        }

        [HttpGet("get-all")]
        [ProducesResponseType(typeof(GlobalResponse<GetUserDto[]>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAll(int page, int perPage, CancellationToken token)
        {
            var users = await patientService.GetAll().Include(k => k.User).Select(c => c.User).ToListAsync(token);

            var mapped = mapper.Map<List<GetUserDto>>(users);

            return Ok(ResponseBuilder.BuildResponse(null, mapped));
        }

        [HttpGet("get-by-id")]
        [ProducesResponseType(typeof(GlobalResponse<GetUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([Required] string userId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError($"BadRequest", "UserId cannot be null or empty");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            return new ControllerResponse().ReturnResponse(await userService.GetUserById(userId, token));
        }

        [HttpGet("get-all-records")]
        [ProducesResponseType(typeof(GlobalResponse<GetRecordDto[]>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAllRecords(int page, int perPage, CancellationToken token)
        {
            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var records = await recordService.GetAll()
                .Where(c => c.PatientId == loggedInUser).ToListAsync(token);

            var mapped = mapper.Map<List<GetRecordDto>>(records);

            return Ok(ResponseBuilder.BuildResponse(null, mapped));
        }

        [HttpGet("get-records-by-medicalOfficer")]
        [ProducesResponseType(typeof(GlobalResponse<GetRecordDto[]>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Role3")]
        public async Task<IActionResult> GetByPstientRecords([Required] string medicalOfficerId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(medicalOfficerId))
            {
                ModelState.AddModelError($"BadRequest", "Medical OfficerId cannot be null or empty");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var records = await recordService.GetAll().Where(c => c.MedicalOfficerId == medicalOfficerId && c.PatientId == loggedInUser).ToListAsync(token);

            var mapped = mapper.Map<List<GetRecordDto>>(records);

            return Ok(ResponseBuilder.BuildResponse(null, mapped));
        }
    }
}
