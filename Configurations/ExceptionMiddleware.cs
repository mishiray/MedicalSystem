using MedicalSystem.DTOs;
using MedicalSystem.Utilities;

namespace MedicalSystem.Configurations
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var response = context.Response;
                response.ContentType = "application/json";
                var responseModel = GlobalResponse<string>.Fail(exception.InnerException?.Message ?? exception.Message);
                switch (exception)
                {
                    case KeyNotFoundException:
                        response.StatusCode = StatusCodes.Status404NotFound;
                        break;
                    case BadRequestException:
                        response.StatusCode = StatusCodes.Status400BadRequest;
                        break;
                    case UnauthorizedAccessException:
                        response.StatusCode = StatusCodes.Status400BadRequest;
                        break;
                    default:
                        response.StatusCode = StatusCodes.Status500InternalServerError;
                        break;
                }

                await response.WriteAsJsonAsync(responseModel);
            }
        }
    }
}
