using MedicalSystem.DTOs.ServiceDtos;
using MedicalSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Services
{
    public interface IPatientService
    {
        Task<CustomResponse<Patient>> Create(Patient patient, CancellationToken token);
        Task<CustomResponse<Patient>> Update(Patient patient, CancellationToken token);
        Task<CustomResponse<Patient>> Get(string patientId, CancellationToken token);
        IQueryable<Patient> GetAll();
    }

    public class PatientService : IPatientService
    {
        private readonly IUserService userService;
        private readonly IRepository repository;

        public PatientService(IUserService userService, IRepository repository)
        {
            this.userService = userService;
            this.repository = repository;
        }

        private async Task<bool> CreateUser(Patient patient, CancellationToken token)
        {
            if(patient == null)
            {
                return false;
            }

            return await repository.AddAsync(patient, token);
        }

        public async Task<CustomResponse<Patient>> Create(Patient patient, CancellationToken token)
        {
            if (patient is null)
            {
                return new CustomResponse<Patient>(DTOs.Enums.ServiceResponses.BadRequest, null, "Patient cannot be null");
            }

            var result = await userService.CreateUser(patient.User);
            if (result.Response == DTOs.Enums.ServiceResponses.Success)
            {
                patient.UserId = result.Data.Id;
                patient.Id = result.Data.Id;

                var createdResult = await CreateUser(patient, token);

                if (createdResult)
                {
                    return await Get(patient.Id, token);
                }
            }

            return new CustomResponse<Patient>(DTOs.Enums.ServiceResponses.Failed, null, "Unable to add");
        }

        public async Task<CustomResponse<Patient>> Get(string patientId, CancellationToken token)
        {
            var user =  await GetAll()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == patientId, token);

            if (user is null)
            {
                return new CustomResponse<Patient>(DTOs.Enums.ServiceResponses.NotFound, null, "User not found");
            }

            return new CustomResponse<Patient>(DTOs.Enums.ServiceResponses.Success, user, null);
        }

        public IQueryable<Patient> GetAll()
        {
            return repository.ListAll<Patient>();
        }

        public async Task<CustomResponse<Patient>> Update(Patient patient, CancellationToken token)
        {
            if (patient is null)
            {
                return new CustomResponse<Patient>(DTOs.Enums.ServiceResponses.BadRequest, null, "Patient cannot be null");
            }

            var result =  await userService.UpdateUser(patient.User);
            if(result.Response == DTOs.Enums.ServiceResponses.Success)
            {
                return await Get(patient.Id, token);
            }

            return new CustomResponse<Patient>(DTOs.Enums.ServiceResponses.Failed, null, "Unable to update");
        }
    }
}
