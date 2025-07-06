using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.Users;
using NewsAggregationSystem.Service.Services;
using NewsAggregationSystem.Service.Tests.Utilities;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace NewsAggregationSystem.Service.Tests.Services
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IMapper> mockMapper;
        private Mock<IUserRepository> mockUserRepository;
        private Mock<IConfiguration> mockConfiguration;
        private Mock<IRepositoryBase<UserRole>> mockUserRoleRepository;
        private Mock<IHttpContextAccessor> mockHttpContextAccessor;
        private Mock<HttpContext> mockHttpContext;
        private Mock<HttpResponse> mockHttpResponse;
        private AuthService authService;

        [SetUp]
        public void SetUp()
        {
            mockMapper = new Mock<IMapper>();
            mockUserRepository = new Mock<IUserRepository>();
            mockConfiguration = new Mock<IConfiguration>();
            mockUserRoleRepository = new Mock<IRepositoryBase<UserRole>>();
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            var mockCookies = new Mock<IResponseCookies>();
            mockCookies
                .Setup(cookies => cookies.Append(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CookieOptions>()));

            mockHttpResponse = new Mock<HttpResponse>();
            mockHttpResponse
                .Setup(r => r.Cookies)
                .Returns(mockCookies.Object);

            mockHttpContext = new Mock<HttpContext>();
            mockHttpContext
                .Setup(ctx => ctx.Response)
                .Returns(mockHttpResponse.Object);

            mockHttpContextAccessor
                .Setup(x => x.HttpContext)
                .Returns(mockHttpContext.Object);

            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x.Value).Returns("JsonWebApiTokenWithSwaggerAuthorizationAuthenticationAspNetCore12345678901234567890");
            mockConfiguration.Setup(x => x.GetSection("Jwt:key")).Returns(jwtSection.Object);
            mockConfiguration.Setup(x => x["Jwt:Subject"]).Returns("NewsAggregationSystem");

            authService = new AuthService(
                mockMapper.Object,
                mockUserRepository.Object,
                mockConfiguration.Object,
                mockUserRoleRepository.Object,
                mockHttpContextAccessor.Object
            );
        }


        [Test]
        public async Task Login_WhenUserExistsAndPasswordIsValid_ReturnsAuthResponse()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash(loginRequest.Password);
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    RoleId = (int)UserRoles.User,
                    Role = new Role { Name = UserRoles.User.ToString() }
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .Returns(userRoles.AsQueryable().BuildMockDbSet().Object);

            var result = await authService.Login(loginRequest);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(ApplicationConstants.TokenType, result.TokenType);
            Assert.AreEqual(user.UserName, result.UserName);
            Assert.AreEqual(UserRoles.User.ToString(), result.Roles);
            Assert.AreEqual(UserRoles.User.ToString(), result.RedirectTo);
            Assert.IsNotNull(result.ExpiresIn);
            Assert.IsNotNull(result.IssuedAt);

            mockHttpResponse.Verify(x => x.Cookies.Append(
                "accessToken",
                It.IsAny<string>(),
                It.IsAny<CookieOptions>()
            ), Times.Once);
        }

        [Test]
        public async Task Login_WhenUserExistsWithMultipleRoles_ReturnsAllRoles()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash(loginRequest.Password);
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "multiuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    RoleId = (int)UserRoles.User,
                    Role = new Role { Name = UserRoles.User.ToString() }
                },
                new UserRole
                {
                    UserId = 1,
                    RoleId = (int)UserRoles.Admin,
                    Role = new Role { Name = UserRoles.Admin.ToString() }
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .Returns(userRoles.AsQueryable().BuildMockDbSet().Object);


            var result = await authService.Login(loginRequest);

            Assert.IsNotNull(result);
            Assert.AreEqual($"{UserRoles.User},{UserRoles.Admin}", result.Roles);
        }

        [Test]
        public void Login_WhenUserDoesNotExist_ThrowsNotFoundException()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);


            var exception = Assert.ThrowsAsync<NotFoundException>(async () => await authService.Login(loginRequest));
            Assert.IsTrue(exception.Message.Contains(loginRequest.Email));
        }

        [Test]
        public void Login_WhenPasswordIsInvalid_ThrowsInvalidCredentialsException()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash("CorrectPassword123!");
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);


            var exception = Assert.ThrowsAsync<InvalidCredentialsException>(async () => await authService.Login(loginRequest));
            Assert.IsTrue(exception.Message.Contains(ApplicationConstants.InvalidPassword));
        }

        [Test]
        public async Task Login_WhenUserExists_ReturnsTokenWithCorrectExpiration()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash(loginRequest.Password);
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    Role = new Role { Name = UserRoles.User.ToString() }
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .Returns(userRoles.AsQueryable().BuildMockDbSet().Object);

            mockConfiguration.Setup(c => c["Jwt:key"])
                .Returns("JsonWebApiTokenWithSwaggerAuthorizationAuthenticationAspNetCore12345678901234567890");

            mockConfiguration.Setup(c => c["Jwt:Subject"])
                .Returns("test-subject");


            var jwtSection = new Mock<IConfigurationSection>();
            jwtSection.Setup(x => x.Value).Returns("JsonWebApiTokenWithSwaggerAuthorizationAuthenticationAspNetCore12345678901234567890");

            var result = await authService.Login(loginRequest);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AccessToken);

            Assert.IsNotNull(result.ExpiresIn);


            var expectedExpiration = DateTimeHelper.GetInstance().CurrentUtcDateTime.AddMinutes(ApplicationConstants.AccessTokenExpireTime);
            var actualExpiration = DateTime.Parse(result.ExpiresIn);

            Assert.That(actualExpiration, Is.EqualTo(expectedExpiration).Within(TimeSpan.FromSeconds(10)));
            Assert.AreEqual(user.UserName, result.UserName);
            Assert.AreEqual(UserRoles.User.ToString(), result.Roles);
            Assert.AreEqual(UserRoles.User.ToString(), result.RedirectTo);
            Assert.AreEqual(ApplicationConstants.TokenType, result.TokenType);
            Assert.IsNotNull(result.IssuedAt);
        }

        [Test]
        public async Task Login_WhenUserExists_SetsHttpOnlyCookie()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash(loginRequest.Password);
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    Role = new Role { Name = UserRoles.User.ToString() }
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .Returns(userRoles.AsQueryable().BuildMockDbSet().Object);


            var result = await authService.Login(loginRequest);


            mockHttpResponse.Verify(x => x.Cookies.Append(
                "accessToken",
                It.IsAny<string>(),
                It.Is<CookieOptions>(options =>
                    options.HttpOnly == true &&
                    options.Expires.HasValue &&
                    options.Expires.Value > DateTime.Now
                )
            ), Times.Once);
        }

        [Test]
        public async Task Login_WhenUserExists_ReturnsTokenWithCorrectClaims()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash(loginRequest.Password);
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    Role = new Role { Name = UserRoles.User.ToString() }
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .Returns(userRoles.AsQueryable().BuildMockDbSet().Object);

            var result = await authService.Login(loginRequest);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(user.UserName, result.UserName);
        }

        [Test]
        public async Task Login_WhenUserExists_ReturnsTokenWithCorrectClaim()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash(loginRequest.Password);
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    Role = new Role { Name = UserRoles.User.ToString() }
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .Returns(userRoles.AsQueryable().BuildMockDbSet().Object);


            var result = await authService.Login(loginRequest);


            Assert.IsNotNull(result);
            Assert.IsNotNull(result.AccessToken);
            Assert.AreEqual(user.UserName, result.UserName);
            Assert.IsNotNull(result.IssuedAt);
            Assert.IsNotNull(result.ExpiresIn);
            Assert.AreEqual(UserRoles.User.ToString(), result.Roles);
            Assert.AreEqual(UserRoles.User.ToString(), result.RedirectTo);
            Assert.AreEqual(ApplicationConstants.TokenType, result.TokenType);
        }

        [Test]
        public void Login_WhenEmailIsEmpty_ThrowsNotFoundException()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "",
                Password = "Sneha@123"
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User>().AsQueryable().BuildMockDbSet().Object);


            var exception = Assert.ThrowsAsync<NotFoundException>(async () => await authService.Login(loginRequest));
            Assert.IsTrue(exception.Message.Contains(loginRequest.Email));
        }

        [Test]
        public void Login_WhenPasswordIsEmpty_ThrowsInvalidCredentialsException()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = ""
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash("Sneha@123");
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);


            var exception = Assert.ThrowsAsync<InvalidCredentialsException>(async () => await authService.Login(loginRequest));
            Assert.IsTrue(exception.Message.Contains(ApplicationConstants.InvalidPassword));
        }

        [Test]
        public void Login_WhenHttpContextIsNull_ThrowsNullReferenceException()
        {

            var loginRequest = new LoginRequestDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123"
            };

            var (passwordHash, passwordSalt) = CreatePasswordHash(loginRequest.Password);
            var user = new User
            {
                Id = 1,
                Email = loginRequest.Email,
                UserName = "testuser",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    UserId = 1,
                    Role = new Role { Name = UserRoles.User.ToString() }
                }
            };

            mockUserRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<User, bool>>>()))
                .Returns(new List<User> { user }.AsQueryable().BuildMockDbSet().Object);

            mockUserRoleRepository
                .Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<UserRole, bool>>>()))
                .Returns(userRoles.AsQueryable().BuildMockDbSet().Object);


            mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null);

            Assert.ThrowsAsync<NullReferenceException>(async () => await authService.Login(loginRequest));
        }

        #region Private Helper Methods
        private (byte[] passwordHash, byte[] passwordSalt) CreatePasswordHash(string password)
        {
            using (var hmac = new HMACSHA512())
            {
                var passwordSalt = hmac.Key;
                var passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return (passwordHash, passwordSalt);
            }
        }
        #endregion
    }
}