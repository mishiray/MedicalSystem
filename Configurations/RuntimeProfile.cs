using AutoMapper;
using MedicalSystem.DTOs;
using MedicalSystem.DTOs.ControllerDtos;
using MedicalSystem.Entities;

namespace MedicalSystem.Configurations
{
    public  class RuntimeProfile : Profile
    {
        public RuntimeProfile()
        {

            #region User Mappings
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.UserName, option => option
                .MapFrom(src => src.Email));
            CreateMap<CreateUserDto, MedicalOfficer>()
                .ForMember(dest => dest.User, option => option
                .MapFrom(src => new User()
                {
                    Name = src.Name,
                    Roles = src.Roles,
                    Password = src.Password,
                    UserName = src.Email,
                    UserType = UserType.MedicalOfficer,
                    Email = src.Email,
                    PhoneNumber = src.PhoneNumber,
                }));
            CreateMap<CreateUserDto, Patient>()
                .ForMember(dest => dest.User, option => option
                .MapFrom(src => new User()
                {
                    Name = src.Name,
                    Roles = src.Roles,
                    Password = src.Password,
                    UserType = UserType.Patient,
                    UserName = src.Email,
                    Email = src.Email,
                    PhoneNumber = src.PhoneNumber
                }));

            CreateMap<User, GetUserDto>();
            #endregion

            #region Records Mappings
            CreateMap<CreateRecordDto, Record>();
            CreateMap<UpdateRecordDto, Record>();
            CreateMap<Record, GetRecordDto>()
                .ForMember(c => c.MedicalOfficer, option => option
                .MapFrom(c => c.MedicalOfficer == null ? null : new GetUserDto()
                {
                    Id = c.Id,
                    DateCreated = c.MedicalOfficer.DateCreated,
                    Email = c.MedicalOfficer.User.Email,
                    Name = c.MedicalOfficer.User.Name,
                    Roles = c.MedicalOfficer.User.Roles,
                    UserType = c.MedicalOfficer.User.UserType.ToString(),
                }))
                .ForMember(c => c.Patient, option => option
                .MapFrom(c => c.Patient == null ? null : new GetUserDto()
                {
                    Id = c.Id,
                    DateCreated = c.Patient.DateCreated,
                    Email = c.Patient.User.Email,
                    Name = c.Patient.User.Name,
                    Roles = c.Patient.User.Roles,
                    UserType = c.Patient.User.UserType.ToString(),
                }));
            #endregion

        }
    }
}
