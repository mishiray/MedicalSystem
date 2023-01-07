using AutoMapper;
using MedicalSystem.DTOs.ControllerDtos;
using MedicalSystem.DTOs.Enums;
using MedicalSystem.DTOs.ServiceDtos;
using MedicalSystem.Entities;
using MedicalSystem.Services;
using MedicalSystem.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MedicalSystem.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class RecordsController : ControllerBase
    {
        private readonly IRecordService recordService;
        private readonly IMapper mapper;
        private readonly IPatientService patientService;
        private readonly IMedicalOfficerService medicalOfficerService;

        public RecordsController(IRecordService recordService, IMapper mapper, IPatientService patientService, IMedicalOfficerService medicalOfficerService)
        {
            this.recordService = recordService;
            this.mapper = mapper;
            this.patientService = patientService;
            this.medicalOfficerService = medicalOfficerService;
        }

        [HttpPost("create")]
        [Authorize(Roles = "Role2")]
        public async Task<IActionResult> Create(CreateRecordDto model, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));


            var record = mapper.Map<Record>(model);

            record.MedicalOfficerId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var createdResult = await recordService.Create(record, token);

            return new ControllerResponse().ReturnResponse(createdResult);
        }

        [HttpGet("get-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ListAll(int page, int perPage, CancellationToken token)
        {
            
            var users = medicalOfficerService.GetAll();

            var paginatedUsers = users.Paginate(page, perPage);

            var mapped = mapper.Map<List<GetUserDto>>(paginatedUsers);

            return Ok(ResponseBuilder.BuildResponse(null, Pagination.GetPagedData(mapped, page, perPage, await users.CountAsync(token))));
        }

        [HttpGet("get-by-id")]
        public async Task<IActionResult> GetById([Required] string recordId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(recordId))
            {
                ModelState.AddModelError($"BadRequest", "RecordId cannot be null or empty");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var result = await recordService.Get(recordId, token);

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

                    if(result.Data.MedicalOfficerId != loggedInUser && result.Data.PatientId != loggedInUser)
                    {
                        ModelState.AddModelError("Unauthorized", "You cannot perform this action");
                        return Unauthorized(ResponseBuilder.BuildResponse<object>(ModelState, null));
                    }

                    return Ok(ResponseBuilder.BuildResponse<object>(null, mapper.Map<GetRecordDto>(result.Data)));

                default:
                    ModelState.AddModelError($"{result.Response}", result.Message);
                    return UnprocessableEntity(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

        }

    }
}
