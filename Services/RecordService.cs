using MedicalSystem.DTOs.ServiceDtos;
using MedicalSystem.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalSystem.Services
{
    public interface IRecordService
    {
        Task<CustomResponse<Record>> Create(Record record, CancellationToken token);
        Task<CustomResponse<Record>> Update(Record record, CancellationToken token);
        Task<CustomResponse<Record>> Get(string recordId, CancellationToken token);
        IQueryable<Record> GetAll();
    }
    public class RecordService : IRecordService
    {
        private readonly IRepository _repository;

        public RecordService(IRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomResponse<Record>> Create(Record record, CancellationToken token)
        {
            if(record is null)
            {
                return new CustomResponse<Record>(DTOs.Enums.ServiceResponses.BadRequest, null, "Record cannot be null");
            }

            var result = await _repository.AddAsync(record, token);
            if (result)
            {
                return await Get(record.Id, token);
            }

            return new CustomResponse<Record>(DTOs.Enums.ServiceResponses.Failed, null, "Unable to add");
        }

        public async Task<CustomResponse<Record>> Get(string recordId, CancellationToken token)
        {
            if (recordId is null)
            {
                return new CustomResponse<Record>(DTOs.Enums.ServiceResponses.BadRequest, null, "Record Id cannot be null");
            }

            var record = await GetAll()
                .Include(c => c.MedicalOfficer.User)
                .Include(c => c.Patient.User)
                .FirstOrDefaultAsync(c => c.Id == recordId, token);

            if(record is null)
            {
                return new CustomResponse<Record>(DTOs.Enums.ServiceResponses.NotFound, null, "Record not found");
            }

            return new CustomResponse<Record>(DTOs.Enums.ServiceResponses.Success, record, null); 
        }

        public IQueryable<Record> GetAll()
        {
            return _repository.ListAll<Record>();
        }

        public async Task<CustomResponse<Record>> Update(Record record, CancellationToken token)
        {
            if (record is null)
            {
                return new CustomResponse<Record>(DTOs.Enums.ServiceResponses.BadRequest, null, "Record cannot be null");
            }

            record.DateModified = DateTime.UtcNow;

            var result = await _repository.ModifyAsync(record, token);
            if (result)
            {
                return await Get(record.Id, token);
            }

            return new CustomResponse<Record>(DTOs.Enums.ServiceResponses.Failed, null, "Unable to update");
        }
    }
}
