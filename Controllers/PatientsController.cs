using AutoMapper;
using MedicalSystem.Controllers;
using MedicalSystem.DTOs;
using MedicalSystem.DTOs.ControllerDtos;
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
        private readonly IMapper mapper;

        public PatientsController(IPatientService patientService, IMapper mapper, IRecordService recordService)
        {
            this.patientService = patientService;
            this.mapper = mapper;
            this.recordService = recordService;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(GlobalResponse<Patient>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin,Role1,Role2")]
        public async Task<IActionResult> Create(CreateUserDto model, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));

            var user = mapper.Map<Patient>(model);

            var createdResult = await patientService.Create(user, token);

            return new ControllerResponse().ReturnResponse(createdResult);
        }

        [HttpGet("get-all")]
        [ProducesResponseType(typeof(GlobalResponse<GetUserDto[]>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAll(int page, int perPage, CancellationToken token)
        {
            var users = patientService.GetAll().Include(k => k.User);

            var paginatedUsers = users.Paginate(page, perPage);

            var mapped = mapper.Map<List<GetUserDto>>(paginatedUsers);

            return Ok(ResponseBuilder.BuildResponse(null, Pagination.GetPagedData(mapped, page, perPage, await users.CountAsync(token))));
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

            return new ControllerResponse().ReturnResponse(await patientService.Get(userId, token));
        }

        [HttpGet("get-all-records")]
        [ProducesResponseType(typeof(GlobalResponse<GetRecordDto[]>), StatusCodes.Status200OK)]
        [Authorize (Roles = "Role3")]
        public async Task<IActionResult> ListAllRecords(int page, int perPage, CancellationToken token)
        {
            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = recordService.GetAll()
                .Where(c => c.PatientId == loggedInUser);

            var paginatedUsers = users.Paginate(page, perPage);

            var mapped = mapper.Map<List<GetRecordDto>>(paginatedUsers);

            return Ok(ResponseBuilder.BuildResponse(null, Pagination.GetPagedData(mapped, page, perPage, await users.CountAsync(token))));
        }

        [HttpGet("get-records-by-medicalOfficer")]
        [ProducesResponseType(typeof(GlobalResponse<GetRecordDto[]>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Role3")]
        public async Task<IActionResult> GetByPstientRecords([Required] string medicalOfficerId, int page, int perPage, CancellationToken token)
        {
            if (string.IsNullOrEmpty(medicalOfficerId))
            {
                ModelState.AddModelError($"BadRequest", "Medical OfficerId cannot be null or empty");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var records = recordService.GetAll().Where(c => c.MedicalOfficerId == medicalOfficerId && c.PatientId == loggedInUser);

            var paginatedRecords = records.Paginate(page, perPage);

            var mapped = mapper.Map<List<GetRecordDto>>(paginatedRecords);

            var paginatedResults = Pagination.GetPagedData(mapped, page, perPage, await records.CountAsync(token));

            return Ok(ResponseBuilder.BuildResponse(null, paginatedResults));
        }
    }
}
