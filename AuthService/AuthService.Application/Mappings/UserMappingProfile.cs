using AutoMapper;
using AuthService.Application.DTOs;
using AuthService.Domain.Entities;

namespace AuthService.Application.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            CreateMap<User, RegisterUserResponse>()
                .ConstructUsing(src => new RegisterUserResponse(
                    src.Id,
                    src.Name,
                    src.Email.Value,
                    string.Empty,
                    DateTime.MinValue
                ));
        }        
    }
}