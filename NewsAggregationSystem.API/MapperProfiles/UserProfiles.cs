using AutoMapper;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.MapperProfiles
{
    public class UserProfiles : Profile
    {
        public UserProfiles()
        {
            CreateMap<UserRequestDTO, User>().ReverseMap();
            CreateMap<UserResponseDTO, User>().ReverseMap();
        }
    }
}
