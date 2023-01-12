using MedicalSystem.DTOs.ServiceDtos;
using MedicalSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Services
{
    public interface IMedicalOfficerService
    {
        Task<CustomResponse<MedicalOfficer>> Create(MedicalOfficer medicalOfficer, CancellationToken token);
        Task<bool> CreateUser(MedicalOfficer medicalOfficer, CancellationToken token);
        Task<CustomResponse<MedicalOfficer>> Update(MedicalOfficer medicalOfficer, CancellationToken token);
        Task<CustomResponse<MedicalOfficer>> Get(string medicalOfficerId, CancellationToken token);
        IQueryable<MedicalOfficer> GetAll();
    }

    public class MedicalOfficerService : IMedicalOfficerService
    {
        private readonly IUserService userService;
        private readonly IRepository repository;

        public MedicalOfficerService(IUserService userService, IRepository repository)
        {
            this.userService = userService;
            this.repository = repository;
        }

        public async Task<bool> CreateUser(MedicalOfficer medicalOfficer, CancellationToken token)
        {
            if(medicalOfficer == null)
            {
                return false;
            }

            return await repository.AddAsync(medicalOfficer, token);
        }

        public async Task<CustomResponse<MedicalOfficer>> Create(MedicalOfficer medicalOfficer, CancellationToken token)
        {
            if (medicalOfficer is null)
            {
                return new CustomResponse<MedicalOfficer>(DTOs.Enums.ServiceResponses.BadRequest, null, "Medical Officer cannot be null");
            }

            var result = await userService.CreateUser(medicalOfficer.User);
            if (result.Response == DTOs.Enums.ServiceResponses.Success)
            {
                medicalOfficer.UserId = result.Data.Id;
                medicalOfficer.Id = result.Data.Id;

                var createdResult = await CreateUser(medicalOfficer, token);

                if (createdResult)
                {
                    return await Get(medicalOfficer.Id, token);
                }
            }

            return new CustomResponse<MedicalOfficer>(DTOs.Enums.ServiceResponses.Failed, null, "Unable to add");
        }

        public async Task<CustomResponse<MedicalOfficer>> Get(string medicalOfficerId, CancellationToken token)
        {
            var user =  await GetAll()
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == medicalOfficerId, token);

            if (user is null)
            {
                return new CustomResponse<MedicalOfficer>(DTOs.Enums.ServiceResponses.NotFound, null, "User not found");
            }

            return new CustomResponse<MedicalOfficer>(DTOs.Enums.ServiceResponses.Success, user, null);
        }

        public IQueryable<MedicalOfficer> GetAll()
        {
            return repository.ListAll<MedicalOfficer>();
        }

        public async Task<CustomResponse<MedicalOfficer>> Update(MedicalOfficer medicalOfficer, CancellationToken token)
        {
            if (medicalOfficer is null)
            {
                return new CustomResponse<MedicalOfficer>(DTOs.Enums.ServiceResponses.BadRequest, null, "Medical Officer cannot be null");
            }

            var result =  await userService.UpdateUser(medicalOfficer.User);
            if(result.Response == DTOs.Enums.ServiceResponses.Success)
            {
                return await Get(medicalOfficer.Id, token);
            }

            return new CustomResponse<MedicalOfficer>(DTOs.Enums.ServiceResponses.Failed, null, "Unable to update");
        }
    }
}
