using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Providers;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.Users;
using NewsAggregationSystem.Service.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace NewsAggregationSystem.Service.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IRepositoryBase<UserRole> userRoleRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        private readonly INotificationPreferenceService notificationPreferenceService;

        public UserService(IUserRepository userRepository, IMapper mapper, IRepositoryBase<UserRole> userRoleRepository, INotificationPreferenceService notificationPreferenceService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.userRoleRepository = userRoleRepository;
            this.notificationPreferenceService = notificationPreferenceService;
        }

        public async Task<List<NotificationEmailDTO>> GenerateNotificationEmails(List<Notification> notifications)
        {
            if (notifications == null || !notifications.Any())
                return new List<NotificationEmailDTO>();

            var userIds = notifications.Select(n => n.UserId).Distinct().ToList();

            var users = await userRepository.GetWhere(user => userIds.Contains(user.Id)).ToListAsync();

            var emailList = (from user in users
                             join notification in notifications on user.Id equals notification.UserId
                             where !string.IsNullOrWhiteSpace(user.Email)
                             select new NotificationEmailDTO
                             {
                                 Email = user.Email,
                                 Subject = notification.Title,
                                 Body = notification.Message
                             }).ToList();
            return emailList;
        }

        public async Task<int> AddUser(UserRequestDTO userRequestData)
        {
            var existingUserWithEmail = await userRepository.GetWhere(user => user.Email.ToLower() == userRequestData.Email.ToLower()).FirstOrDefaultAsync();
            var existingUserWithUserName = await userRepository.GetWhere(user => user.UserName.ToLower() == userRequestData.UserName.ToLower()).FirstOrDefaultAsync();
            if (existingUserWithEmail != null || existingUserWithUserName != null)
            {
                throw new AlreadyExistException(string.Format(ApplicationConstants.UserAlreadyExistWithThisEmail, userRequestData.Email, userRequestData.UserName));
            }
            var user = mapper.Map<User>(userRequestData);
            byte[] passwordHash, passwordSalt;

            CreatePasswordHash(userRequestData.Password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.CreatedDate = dateTimeHelper.CurrentUtcDateTime;
            await userRepository.AddAsync(user);
            await userRoleRepository.AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = (int)UserRoles.User,
                CreatedById = user.Id,
                CreatedDate = dateTimeHelper.CurrentUtcDateTime
            });
            await notificationPreferenceService.AddNotificationPreferencesPerUser(user.Id);
            return user.Id;
        }

        public async Task<List<UserResponseDTO>> GetAllUsers()
        {
            return mapper.Map<List<UserResponseDTO>>(await userRepository.GetAll().ToListAsync());
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
    }
}
