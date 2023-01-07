using MedicalSystem.DTOs.Enums;
using MedicalSystem.DTOs.ServiceDtos;
using MedicalSystem.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MedicalSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControllerResponse : ControllerBase
    {
        public IActionResult ReturnResponse<T>(CustomResponse<T> customResponse)
        {
            switch (customResponse.Response)
            {
                case ServiceResponses.BadRequest:
                    ModelState.AddModelError($"{customResponse.Response}", customResponse.Message);
                    return BadRequest(ResponseBuilder.BuildResponse<object>(ModelState, null));

                case ServiceResponses.NotFound:
                    ModelState.AddModelError($"{customResponse.Response}", customResponse.Message);
                    return NotFound(ResponseBuilder.BuildResponse<object>(ModelState, null));

                case ServiceResponses.Failed:
                    ModelState.AddModelError($"{customResponse.Response}", customResponse.Message);
                    return UnprocessableEntity(ResponseBuilder.BuildResponse<object>(ModelState, null));

                case ServiceResponses.Success:
                    return Ok(ResponseBuilder.BuildResponse<object>(null, customResponse.Data == null ? customResponse.Response : customResponse.Data));

                default:
                    ModelState.AddModelError($"{customResponse.Response}", customResponse.Message);
                    return UnprocessableEntity(ResponseBuilder.BuildResponse<object>(ModelState, null));
            }
        }
    }
}
