using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using AutoMapper;
using API.Entities;
using API.Extensions;

namespace API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberDto>()
            .ForMember(dest=> dest.PhotoUrl, opt=> opt
            .MapFrom(src=>src.Photos.FirstOrDefault(b=>b.IsMain).Url))
            .ForMember(dest=> dest.Age, opt=>opt
            .MapFrom(src=>src.DateOfBirth.CalculateAge()));
            CreateMap<Photo,PhotoDto>();
            CreateMap<MemberUpdateDto,AppUser>();
            CreateMap<RegisterDto,AppUser>();
        }
    }
}