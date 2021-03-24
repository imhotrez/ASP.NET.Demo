using AutoMapper;
using Demo.Models.Domain.Auth;
using Demo.Models.Dto;

namespace Demo.WebAPI {
    public class MappingProfile : Profile {
        public MappingProfile() {
            CreateMap<AppUser, AppUserDto>()
                .ForMember(appUserDto => appUserDto.Id, p => p.MapFrom(appUser => appUser.Id))
                .ForMember(appUserDto => appUserDto.Email, p => p.MapFrom(appUser => appUser.Email))
                .ForMember(appUserDto => appUserDto.FirstName, p => p.MapFrom(appUser => appUser.FirstName))
                .ForMember(appUserDto => appUserDto.MiddleName, p => p.MapFrom(appUser => appUser.MiddleName))
                .ForMember(appUserDto => appUserDto.LastName, p => p.MapFrom(appUser => appUser.LastName))
                .ForMember(appUserDto => appUserDto.PhoneNumber, p => p.MapFrom(appUser => appUser.PhoneNumber));
            CreateMap<AppUserDto, AppUser>()
                .ForMember(appUser => appUser.Id, p => p.MapFrom(appUserDto => appUserDto.Id))
                .ForMember(appUser => appUser.Email, p => p.MapFrom(appUserDto => appUserDto.Email))
                .ForMember(appUser => appUser.FirstName, p => p.MapFrom(appUserDto => appUserDto.FirstName))
                .ForMember(appUser => appUser.MiddleName, p => p.MapFrom(appUserDto => appUserDto.MiddleName))
                .ForMember(appUser => appUser.LastName, p => p.MapFrom(appUserDto => appUserDto.LastName))
                .ForMember(appUser => appUser.PhoneNumber, p => p.MapFrom(appUserDto => appUserDto.PhoneNumber));

            CreateMap<AppRole, AppRoleDto>()
                .ForMember(appRoleDto => appRoleDto.Id, p => p.MapFrom(appRole => appRole.Id))
                .ForMember(appRoleDto => appRoleDto.Name, p => p.MapFrom(appRole => appRole.Name));
            CreateMap<AppRoleDto, AppRole>()
                .ForMember(appRole => appRole.Id, p => p.MapFrom(appRoleDto => appRoleDto.Id))
                .ForMember(appRole => appRole.Name, p => p.MapFrom(appRoleDto => appRoleDto.Name));
        }
    }
}