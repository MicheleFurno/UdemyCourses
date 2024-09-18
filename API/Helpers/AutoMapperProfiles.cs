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
        CreateMap<MemberUpdateDTO, AppUser>();
        CreateMap<RegisterDTO, AppUser>();
        CreateMap<string, DateOnly>().ConvertUsing(source => DateOnly.Parse(source.Substring(0,10)));
        CreateMap<Message, MessageDTO>()
            .ForMember(destination => destination.SenderPhotoUrl, 
                                      option => option.MapFrom(source => source.Sender.Photos.FirstOrDefault(photo => photo.IsMain)!.Url))
            .ForMember(destination => destination.RecipientPhotoUrl, 
                                      option => option.MapFrom(source => source.Recipient.Photos.FirstOrDefault(photo => photo.IsMain)!.Url))
            .ForMember(destination => destination.SenderUserName, 
                                      option => option.MapFrom(source => source.Sender.UserName))
            .ForMember(destination => destination.RecipientUserName, 
                                      option => option.MapFrom(source => source.Recipient.UserName))
        ;
    }
}
