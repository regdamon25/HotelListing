using AutoMapper;
using HotelListing.API.Core.Contracts;
using HotelListing.Data;
using HotelListing.API.Core.Models.Users;

using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HotelListing.Common;

namespace HotelListing.API.Core.Repository
{
    public class AuthManager : IAuthManager
    {
        private readonly IMapper _mapper;
        private readonly UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthManager> _logger;
        private ApiUser _user;


        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration, ILogger<AuthManager> logger)
        {
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }


        public async Task<string> CreateRefreshToken()
        {
            await _userManager.RemoveAuthenticationTokenAsync(_user, SD.LoginProvider, SD.RefreshToken);
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, SD.LoginProvider,
                SD.RefreshToken);
            var result = await _userManager.SetAuthenticationTokenAsync(_user, SD.LoginProvider,
                SD.RefreshToken, newRefreshToken);
            return newRefreshToken;
        }

        public async Task<AuthResponseDTO> Login(LoginDTO loginDTO)
        {
            _logger.LogInformation($"Looking for user wiht email {loginDTO.Email}");
            _user = await _userManager.FindByEmailAsync(loginDTO.Email);
            bool isValidUser = await _userManager.CheckPasswordAsync(_user, loginDTO.Password);
            
            if(_user == null || isValidUser == false)
            {
                _logger.LogWarning($"Looking for user wiht email {loginDTO.Email}");
                return null;
            }

            var token = await GenerateToken();
            _logger.LogWarning($"Token generated for user with email {loginDTO.Email} | Token: {token}");
            return new AuthResponseDTO
            {
                Token = token,
                UserId = _user.Id,
                RefreshToken = await CreateRefreshToken()
            };

        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDTO userDTO)
        {
            _user = _mapper.Map<ApiUser>(userDTO);
            _user.UserName = userDTO.Email;

            var result = await _userManager.CreateAsync(_user, userDTO.Password);

            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }

            return result.Errors;
        }

        public async Task<AuthResponseDTO> VerifyRefreshToken(AuthResponseDTO request)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtSecurityTokenHandler.ReadJwtToken(request.Token);
            var username = tokenContent.Claims.ToList().FirstOrDefault(q => q.Type == JwtRegisteredClaimNames.Email)?.Value;
            _user = await _userManager.FindByNameAsync(username);

            if (_user == null || _user.Id != request.UserId)
            {
                return null;
            }

            var isValidRefreshToken = await _userManager.VerifyUserTokenAsync(_user,
                SD.LoginProvider,
                SD.RefreshToken,
                request.RefreshToken);

            if (isValidRefreshToken)
            {
                var token = await GenerateToken();
                return new AuthResponseDTO
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await CreateRefreshToken()
                };
            }

            await _userManager.UpdateSecurityStampAsync(_user);
            return null;
        }


        private async Task<string> GenerateToken()
        {
            var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration
                ["JwtSettings:Key"]));

            var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(_user);//Going to the database to see what roles this user has

            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();

            var userClaims = await _userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, _user.Email),
                new Claim("uid", _user.Id),
            }
            .Union(userClaims).Union(roleClaims);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
