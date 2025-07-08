using AutoMapper;
using Moq;
using NewsAggregationSystem.Common.DTOs.Users;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.Users;
using NewsAggregationSystem.Service.Interfaces;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class UserServiceTests
    {
        private Mock<IUserRepository> mockUserRepository;
        private Mock<IMapper> mockMapper;
        private Mock<IRepositoryBase<UserRole>> mockUserRoleRepository;
        private Mock<INotificationPreferenceService> mockNotificationPreferenceService;
        private UserService userService;

        [SetUp]
        public void SetUp()
        {
            mockUserRepository = new Mock<IUserRepository>();
            mockMapper = new Mock<IMapper>();
            mockUserRoleRepository = new Mock<IRepositoryBase<UserRole>>();
            mockNotificationPreferenceService = new Mock<INotificationPreferenceService>();

            userService = new UserService(
                mockUserRepository.Object,
                mockMapper.Object,
                mockUserRoleRepository.Object,
                mockNotificationPreferenceService.Object
            );
        }

        #region GenerateNotificationEmails Tests

        [Test]
        public async Task GenerateNotificationEmails_WhenNotificationsExist_ReturnsEmailList()
        {
            var notifications = new List<Notification>
            {
                new Notification
                {
                    Id = 1,
                    UserId = 1,
                    Title = "New Article",
                    Message = "New article available"
                },
                new Notification
                {
                    Id = 2,
                    UserId = 2,
                    Title = "Breaking News",
                    Message = "Breaking news alert"
                }
            };

            var users = new List<User>
            {
                new User { Id = 1, Email = "user1@example.com" },
                new User { Id = 2, Email = "user2@example.com" }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(users.AsQueryable().BuildMockDbSet().Object);

            var result = await userService.GenerateNotificationEmails(notifications);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("user1@example.com", result[0].Email);
            Assert.AreEqual("New Article", result[0].Subject);
            Assert.AreEqual("New article available", result[0].Body);
            Assert.AreEqual("user2@example.com", result[1].Email);
            Assert.AreEqual("Breaking News", result[1].Subject);
            Assert.AreEqual("Breaking news alert", result[1].Body);
        }

        [Test]
        public async Task GenerateNotificationEmails_WhenNotificationsEmpty_ReturnsEmptyList()
        {
            var notifications = new List<Notification>();

            var result = await userService.GenerateNotificationEmails(notifications);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            mockUserRepository.Verify(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()), Times.Never);
        }

        #endregion

        #region AddUser Tests

        [Test]
        public async Task AddUser_WhenValidUserData_AddsUserSuccessfully()
        {
            var userRequest = new UserRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com",
                Password = "Password123!"
            };

            var user = new User
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "john.doe@example.com"
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);

            mockMapper
                .Setup(m => m.Map<User>(userRequest))
                .Returns(user);

            mockUserRepository
                .Setup(repo => repo.AddAsync(It.IsAny<User>()))
                .ReturnsAsync(1);

            mockUserRoleRepository
                .Setup(repo => repo.AddAsync(It.IsAny<UserRole>()))
                .ReturnsAsync(1);

            mockNotificationPreferenceService
                .Setup(service => service.AddNotificationPreferencesPerUser(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            var result = await userService.CreateUserAsync(userRequest);

            Assert.AreEqual(1, result);
            mockUserRepository.Verify(repo => repo.AddAsync(It.Is<User>(u =>
                u.FirstName == userRequest.FirstName &&
                u.LastName == userRequest.LastName &&
                u.UserName == userRequest.UserName &&
                u.Email == userRequest.Email &&
                u.PasswordHash != null &&
                u.PasswordSalt != null
            )), Times.Once);
            mockUserRoleRepository.Verify(repo => repo.AddAsync(It.Is<UserRole>(ur =>
                ur.UserId == 1 &&
                ur.RoleId == (int)UserRoles.User
            )), Times.Once);
            mockNotificationPreferenceService.Verify(service => service.AddNotificationPreferencesPerUser(1), Times.Once);
        }

        [Test]
        public void AddUser_WhenEmailAlreadyExists_ThrowsAlreadyExistException()
        {
            var userRequest = new UserRequestDTO
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "johndoe",
                Email = "existing@example.com",
                Password = "Password123!"
            };

            var existingUser = new User
            {
                Id = 1,
                Email = "existing@example.com",
                UserName = "existinguser"
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
                .Returns(new List<User> { existingUser }.AsQueryable().BuildMockDbSet().Object);

            var exception = Assert.ThrowsAsync<AlreadyExistException>(async () =>
                await userService.CreateUserAsync(userRequest));

            Assert.IsTrue(exception.Message.Contains(userRequest.Email));
            mockUserRepository.Verify(repo => repo.AddAsync(It.IsAny<User>()), Times.Never);
            mockUserRoleRepository.Verify(repo => repo.AddAsync(It.IsAny<UserRole>()), Times.Never);
        }

        #endregion

        #region GetAllUsers Tests

        [Test]
        public async Task GetAllUsers_WhenUsersExist_ReturnsMappedUserList()
        {
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    UserName = "johndoe",
                    Email = "john.doe@example.com"
                },
                new User
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    UserName = "janesmith",
                    Email = "jane.smith@example.com"
                }
            };

            var userDtos = new List<UserResponseDTO>
            {
                new UserResponseDTO
                {
                    Id = 1,
                    FirstName = "John",
                    LastName = "Doe",
                    UserName = "johndoe",
                    Email = "john.doe@example.com"
                },
                new UserResponseDTO
                {
                    Id = 2,
                    FirstName = "Jane",
                    LastName = "Smith",
                    UserName = "janesmith",
                    Email = "jane.smith@example.com"
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetAll())
                .Returns(users.AsQueryable().BuildMockDbSet().Object);

            mockMapper
                .Setup(m => m.Map<List<UserResponseDTO>>(It.IsAny<List<User>>()))
                .Returns(userDtos);

            var result = await userService.GetAllUsers();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("John", result[0].FirstName);
            Assert.AreEqual("Jane", result[1].FirstName);
            mockMapper.Verify(m => m.Map<List<UserResponseDTO>>(It.IsAny<List<User>>()), Times.Once);
        }

        [Test]
        public async Task GetAllUsers_WhenNoUsersExist_ReturnsEmptyList()
        {
            var users = new List<User>();
            var userDtos = new List<UserResponseDTO>();

            mockUserRepository
                .Setup(repo => repo.GetAll())
                .Returns(users.AsQueryable().BuildMockDbSet().Object);

            mockMapper
                .Setup(m => m.Map<List<UserResponseDTO>>(It.IsAny<List<User>>()))
                .Returns(userDtos);

            var result = await userService.GetAllUsers();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            mockMapper.Verify(m => m.Map<List<UserResponseDTO>>(It.IsAny<List<User>>()), Times.Once);
        }

        #endregion
    }
}