using NewsAggregationSystem.Common.DTOs.Providers;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.DAL.Entities;

namespace NewsAggregationSystem.Service.Interfaces
{
    public interface IUserService
    {
        Task<List<NotificationEmailDTO>> GenerateNotificationEmails(List<Notification> notifications);
        Task<int> AddUser(UserRequestDTO user);
        Task<List<UserResponseDTO>> GetAllUsers();
    }
}
