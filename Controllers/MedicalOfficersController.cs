﻿using AutoMapper;
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
    public class MedicalOfficersController : ControllerBase
    {
        private readonly IMedicalOfficerService medicalOfficerService;
        private readonly IRecordService recordService;
        private readonly IUserService userService;
        private readonly IMapper mapper;

        public MedicalOfficersController(IMedicalOfficerService medicalOfficerService, IMapper mapper, IRecordService recordService, IUserService userService)
        {
            this.medicalOfficerService = medicalOfficerService;
            this.mapper = mapper;
            this.recordService = recordService;
            this.userService = userService;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(GlobalResponse<GetUserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [Authorize(Roles ="Admin,Role1")]
        public async Task<IActionResult> Create(CreateUserDto model, CancellationToken token)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));

            var user = mapper.Map<MedicalOfficer>(model);

            var result = await medicalOfficerService.Create(user, token);

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
            var users = medicalOfficerService.GetAll().Include(k => k.User).Select(c => c.User);

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

            return new ControllerResponse().ReturnResponse(await userService.GetUserById(userId, token));
        }

        [HttpGet("get-all-records")]
        [ProducesResponseType(typeof(GlobalResponse<GetRecordDto[]>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAllRecords(int page, int perPage, CancellationToken token)
        {
            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = recordService.GetAll()
                .Where(c => c.MedicalOfficerId == loggedInUser);

            var paginatedUsers = users.Paginate(page, perPage);

            var mapped = mapper.Map<List<GetRecordDto>>(paginatedUsers);

            return Ok(ResponseBuilder.BuildResponse(null, Pagination.GetPagedData(mapped, page, perPage, await users.CountAsync(token))));
        }

        [HttpGet("get-records-by-patient")]
        [ProducesResponseType(typeof(GlobalResponse<GetRecordDto[]>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByPstientRecords([Required] string patientId, int page, int perPage, CancellationToken token)
        {
            if (string.IsNullOrEmpty(patientId))
            {
                ModelState.AddModelError($"BadRequest", "PatientId cannot be null or empty");
                return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }

            var loggedInUser = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var records = recordService.GetAll().Where(c => c.PatientId == patientId && c.MedicalOfficerId == loggedInUser);

            var paginatedRecords = records.Paginate(page, perPage);

            var mapped = mapper.Map<List<GetRecordDto>>(paginatedRecords);

            var paginatedResults = Pagination.GetPagedData(mapped, page, perPage, await records.CountAsync(token));

            return Ok(ResponseBuilder.BuildResponse(null, paginatedResults));
        }
    }
}
