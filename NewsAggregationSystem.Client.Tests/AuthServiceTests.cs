using Moq;
using Moq.Protected;
using NewsAggregationSystem.Client.Services;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.DTOs.Users;
using NUnit.Framework;
using System.Net;
using System.Text;
using System.Text.Json;

namespace NewsAggregationSystem.Client.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<HttpMessageHandler> mockHttpMessageHandler;
        private HttpClient httpClient;
        private AuthService authService;
        private JsonSerializerOptions jsonOptions;

        [SetUp]
        public void Setup()
        {
            this.mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            this.httpClient = new HttpClient(mockHttpMessageHandler.Object);
            this.httpClient.BaseAddress = new Uri(ApplicationConstants.BaseUrl);
            this.authService = new AuthService(httpClient);
            this.jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        [Test]
        public async Task AuthenticateUser_ValidCredentials_ReturnsAuthResponse()
        {
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123!"
            };

            var expectedAuthResponse = new AuthResponseDTO
            {
                AccessToken = "kjasdkfmaksdlca-asdfjknjskdak-asdfcjknaaksd",
                ExpiresIn = "3600"
            };

            var responseContent = JsonSerializer.Serialize(expectedAuthResponse, jsonOptions);

            mockHttpMessageHandler
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

            var result = await authService.AuthenticateUserAsync(loginRequest);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.EqualTo(expectedAuthResponse.AccessToken));
            Assert.That(result.ExpiresIn, Is.EqualTo(expectedAuthResponse.ExpiresIn));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.LoginPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AuthenticateUser_InvalidCredentials_ReturnsNull()
        {
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "invalid@yop.com",
                Password = "lkajdfslkc!"
            };

            mockHttpMessageHandler
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

            var result = await authService.AuthenticateUserAsync(loginRequest);

            Assert.That(result, Is.Null);

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.LoginPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Test]
        public async Task AuthenticateUser_BadRequest_ReturnsNull()
        {
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "adsfcad",
                Password = "short"
            };

            mockHttpMessageHandler
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

            var result = await authService.AuthenticateUserAsync(loginRequest);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AuthenticateUser_NotFound_ReturnsNull()
        {
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "alsdkfjs@yop.com",
                Password = "Sneha@123!"
            };

            mockHttpMessageHandler
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

            var result = await authService.AuthenticateUserAsync(loginRequest);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AuthenticateUser_ServerError_ReturnsNull()
        {
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123!"
            };

            mockHttpMessageHandler
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

            var result = await authService.AuthenticateUserAsync(loginRequest);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task AuthenticateUser_HttpException_ReturnsNull()
        {
            var loginRequest = new LoginRequestForClientDTO
            {
                Email = "sneha.joshi@yopmail.com",
                Password = "Sneha@123!"
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var result = await authService.AuthenticateUserAsync(loginRequest);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task RegisterUserAsync_ValidUser_Success()
        {
            var userRequest = new UserRequestDTO
            {
                Email = "himanshu.mishra@yopmail.com",
                Password = "Himanshu@123",
                UserName = "HimanshuHimanshu"
            };

            mockHttpMessageHandler
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

            Assert.DoesNotThrowAsync(async () => await authService.RegisterUserAsync(userRequest));

            mockHttpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri.ToString().EndsWith(ApplicationConstants.SignupPath)),
                ItExpr.IsAny<CancellationToken>());
        }

        [TearDown]
        public void TearDown()
        {
            httpClient?.Dispose();
        }
    }
}