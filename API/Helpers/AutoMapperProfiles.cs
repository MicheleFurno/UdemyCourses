using System;
using API.DTOs;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<AppUser, MemberDTO>().ForMember(destination => destination.PhotoUrl, 
                                                  option => option.MapFrom(source => source.Photos.FirstOrDefault(photo => photo.IsMain)!.Url))
                                        .ForMember(destination => destination.Age, 
                                                   option => option.MapFrom(source => source.DateOfBirth.CalculateAge()));
        CreateMap<Photo, PhotoDTO>();
    }
}
