using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Client.Services.Interfaces;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;
using System.Net;
using System.Text;
using System.Text.Json;
using NUnit.Framework;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private AuthService _authService;
        private JsonSerializerOptions _jsonOptions;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            _authService = new AuthService(_httpClient);
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task AuthenticateUserAsync_ValidCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var expectedAuthResponse = new AuthResponseDTO
            {
                Token = "test-jwt-token",
                RefreshToken = "test-refresh-token",
                ExpiresIn = 3600
            };

            var responseContent = JsonSerializer.Serialize(expectedAuthResponse, _jsonOptions);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, ApplicationConstants.JsonContentType)
                });

            // Act
            var result = await _authService.AuthenticateUserAsync(loginRequest);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.EqualTo(expectedAuthResponse.Token));
            Assert.That(result.RefreshToken, Is.EqualTo(expectedAuthResponse.RefreshToken));
            Assert.That(result.ExpiresIn, Is.EqualTo(expectedAuthResponse.ExpiresIn));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.LoginPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AuthenticateUserAsync_InvalidCredentials_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "invalid@example.com",
                Password = "WrongPassword123!"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Invalid credentials")
                });

            // Act
            var result = await _authService.AuthenticateUserAsync(loginRequest);

            // Assert
            Assert.That(result, Is.Null);

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.LoginPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AuthenticateUserAsync_BadRequest_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "invalid-email",
                Password = "short"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid format")
                });

            // Act
            var result = await _authService.AuthenticateUserAsync(loginRequest);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AuthenticateUserAsync_NotFound_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "nonexistent@example.com",
                Password = "TestPassword123!"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("User not found")
                });

            // Act
            var result = await _authService.AuthenticateUserAsync(loginRequest);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AuthenticateUserAsync_ServerError_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            // Act
            var result = await _authService.AuthenticateUserAsync(loginRequest);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AuthenticateUserAsync_HttpException_ReturnsNull()
        {
            // Arrange
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _authService.AuthenticateUserAsync(loginRequest);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RegisterUserAsync_ValidUser_Success()
        {
            // Arrange
            var userRequest = new UserRequestDTO
            {
                Email = "newuser@example.com",
                Password = "NewPassword123!",
                UserName = "newuser"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("User created successfully")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _authService.RegisterUserAsync(userRequest));

            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.SignupPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task RegisterUserAsync_BadRequest_HandlesError()
        {
            // Arrange
            var userRequest = new UserRequestDTO
            {
                Email = "invalid-email",
                Password = "short",
                UserName = ""
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Invalid input")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _authService.RegisterUserAsync(userRequest));
        }

        [Test]
        public async Task RegisterUserAsync_Conflict_HandlesError()
        {
            // Arrange
            var userRequest = new UserRequestDTO
            {
                Email = "existing@example.com",
                Password = "TestPassword123!",
                UserName = "existinguser"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Content = new StringContent("User already exists")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _authService.RegisterUserAsync(userRequest));
        }

        [Test]
        public async Task RegisterUserAsync_ServerError_HandlesError()
        {
            // Arrange
            var userRequest = new UserRequestDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                UserName = "testuser"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Server error")
                });

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _authService.RegisterUserAsync(userRequest));
        }

        [Test]
        public async Task RegisterUserAsync_HttpException_HandlesError()
        {
            // Arrange
            var userRequest = new UserRequestDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!",
                UserName = "testuser"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            Assert.DoesNotThrowAsync(async () => await _authService.RegisterUserAsync(userRequest));
        }

        [Test]
        public async Task AuthenticateUserAsync_ValidatesRequestContent()
        {
            // Arrange
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            var expectedAuthResponse = new AuthResponseDTO
            {
                Token = "test-jwt-token",
                RefreshToken = "test-refresh-token",
                ExpiresIn = 3600
            };

            var responseContent = JsonSerializer.Serialize(expectedAuthResponse, _jsonOptions);

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, ApplicationConstants.JsonContentType)
                });

            // Act
            await _authService.AuthenticateUserAsync(loginRequest);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.LoginPath) &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task RegisterUserAsync_ValidatesRequestContent()
        {
            // Arrange
            var userRequest = new UserRequestDTO
            {
                Email = "newuser@example.com",
                Password = "NewPassword123!",
                UserName = "newuser"
            };

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("User created successfully")
                });

            // Act
            await _authService.RegisterUserAsync(userRequest);

            // Assert
            _mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.SignupPath) &&
                    req.Content.Headers.ContentType.MediaType == ApplicationConstants.JsonContentType),
                ItExpr.IsAny<CancellationToken>());
        }
    }
} 