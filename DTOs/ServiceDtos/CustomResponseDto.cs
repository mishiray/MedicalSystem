using MedicalSystem.DTOs.Enums;

namespace MedicalSystem.DTOs.ServiceDtos
{
    public class CustomResponse<T>
    {
        public ServiceResponses Response { get; set; }
        public T Data { get; set; }
        public string Message { get; set; }

        public CustomResponse()
        {
        }

        public CustomResponse(ServiceResponses response, T data, string message)
        {
            Response = response;
            Data = data;
            Message = message;
        }
    }
}
