using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewsAggregationSystem.Common.Constants;
using NewsAggregationSystem.Common.DTOs.Authenticate;
using NewsAggregationSystem.Common.Enums;
using NewsAggregationSystem.Common.Exceptions;
using NewsAggregationSystem.Common.Utilities;
using NewsAggregationSystem.DAL.Entities;
using NewsAggregationSystem.DAL.Repositories.Generic;
using NewsAggregationSystem.DAL.Repositories.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace NewsAggregationSystem.API.Services.Authentication
{
    public class AuthService : IAuthService
    {
        private readonly IMapper mapper;
        private readonly IUserRepository userRepository;
        private readonly IConfiguration configuration;
        private readonly IRepositoryBase<UserRole> userRoleRepository;
        private readonly DateTimeHelper dateTimeHelper = DateTimeHelper.GetInstance();
        private readonly IHttpContextAccessor httpContextAccessor;
        public AuthService(IMapper mapper, IUserRepository userRepository, IConfiguration configuration, IRepositoryBase<UserRole> userRoleRepository, IHttpContextAccessor httpContextAccessor)
        {
            this.mapper = mapper;
            this.userRepository = userRepository;
            this.configuration = configuration;
            this.userRoleRepository = userRoleRepository;
            this.httpContextAccessor = httpContextAccessor;

        }
        public async Task<AuthResponseDTO> Login(LoginRequestDTO loginRequest)
        {
            var user = await userRepository.GetWhere(user => user.Email == loginRequest.Email).FirstOrDefaultAsync();

            if (user == null)
            {
                throw new NotFoundException(string.Format(ApplicationConstants.UserNotFoundWithThisEmail, loginRequest.Email));
            }

            if (!VerifyPasswordHash(loginRequest.Password, user.PasswordHash, user.PasswordSalt))
            {
                throw new InvalidCredentialsException(string.Format(ApplicationConstants.InvalidPassword, loginRequest.Password));
            }

            return await CreateToken(user);
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private async Task<AuthResponseDTO> CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"]),
                new Claim("Id",user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
            };

            var roles = await userRoleRepository.GetWhere(userRole => userRole.UserId == user.Id)
                            .Select(userRole => userRole.Role.Name)
                            .ToListAsync();

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var tokenExpireTimeOut = dateTimeHelper.CurrentUtcDateTime.AddMinutes(ApplicationConstants.AccessTokenExpireTime);

            claims.Add(new Claim(ClaimTypes.Expiration, tokenExpireTimeOut.ToString()));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt:key").Value));

            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: tokenExpireTimeOut,
                signingCredentials: cred
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);


            httpContextAccessor.HttpContext.Response.Cookies.Append("accessToken", jwt, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddMinutes(20)
            });

            return new AuthResponseDTO()
            {
                AccessToken = jwt,
                TokenType = ApplicationConstants.TokenType,
                ExpiresIn = tokenExpireTimeOut.ToString(),
                UserName = user.UserName,
                Roles = string.Join(',', roles),
                RedirectTo = roles.First().Equals(UserRoles.Admin.ToString(), StringComparison.OrdinalIgnoreCase) ? UserRoles.Admin.ToString() : UserRoles.User.ToString(),
                IssuedAt = dateTimeHelper.CurrentUtcDateTime.ToString()
            };
        }
    }
}
