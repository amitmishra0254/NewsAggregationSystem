using AutoMapper;
using NewsAggregationSystem.Common.DTOs.Notifications;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.API.MapperProfiles
{
    public class NotificationProfiles : Profile
    {
        public NotificationProfiles()
        {
            CreateMap<GetAllNotificationsDTO, Notification>().ReverseMap();
        }
    }
}
