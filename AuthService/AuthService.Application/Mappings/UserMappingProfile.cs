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
                .ForMember(
                    dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name)
                )
                .ForMember(
                    dest => dest.Email,
                    opt => opt.MapFrom(src => src.Email.Value)
                )
                .ForMember(
                    dest => dest.Token,
                    opt => opt.Ignore()
                )
                .ForMember(
                    dest => dest.ExpiresAt,
                    opt => opt.Ignore()
                );
        }
        
    }
}