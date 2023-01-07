using AutoMapper;
using MedicalSystem.DTOs;
using MedicalSystem.DTOs.ControllerDtos;
using MedicalSystem.DTOs.Enums;
using MedicalSystem.DTOs.ServiceDtos;
using MedicalSystem.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MedicalSystem.Services
{
    public interface IUserService
    {
        public HttpContext HttpContext { get; }
        public SignInManager<User> SignInManager { get; }
        public UserManager<User> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        ValueTask<CustomResponse<AuthResponse>> Login(LoginRequestModel request);
        Task<CustomResponse<GetUserDto>> CreateUser(User user);
        Task<CustomResponse<bool>> UpdateUser(User user);
        Task<CustomResponse<GetUserDto>> GetUserById(string id, CancellationToken token);
        IQueryable<User> ListAll();
        Task<CustomResponse<bool>> Delete(User user);
    }
    public class UserService : IUserService
    {
        private readonly IMapper mapper;

        private readonly JWT _jwt;
        public SignInManager<User> SignInManager { get; }
        public UserManager<User> UserManager { get; }
        public RoleManager<IdentityRole> RoleManager { get; }
        public HttpContext HttpContext { get; }
        public IRepository repository { get; set; }

        public UserService(RoleManager<IdentityRole> roleManager, SignInManager<User> signInManager, UserManager<User> userManager,
            IMapper mapper, IOptions<JWT> options, IRepository repository)
        {
            this.mapper = mapper;
            RoleManager = roleManager;
            SignInManager = signInManager;
            UserManager = userManager;
            _jwt = options.Value;
            this.repository = repository;
        }

        public async Task<CustomResponse<GetUserDto>> CreateUser(User user)
        {
            if (user == null)
                return new CustomResponse<GetUserDto>(ServiceResponses.BadRequest, null, "User cannot be null");

            if ((await UserManager.FindByEmailAsync(user.Email)) is not null)
                return new CustomResponse<GetUserDto>(ServiceResponses.BadRequest, null, "User with email already exists");

            if (await ListAll().FirstOrDefaultAsync(c => c.PhoneNumber == user.PhoneNumber) is not null)
                return new CustomResponse<GetUserDto>(ServiceResponses.BadRequest, null, "User with phone number already exists");

            foreach(var role in user.Roles)
            {
                if (await RoleManager.FindByNameAsync(role) is null)
                    return new CustomResponse<GetUserDto>(ServiceResponses.BadRequest, null, $"Role {role} not found");
            };

            var createResult = await UserManager.CreateAsync(user, user.Password);

            if (createResult.Succeeded)
            {
                var existingUser = await UserManager.FindByEmailAsync(user.Email);
                await UserManager.AddToRolesAsync(existingUser, existingUser.Roles);
                return new CustomResponse<GetUserDto>(ServiceResponses.Success, mapper.Map<GetUserDto>(existingUser), null);
            }

            return new CustomResponse<GetUserDto>(ServiceResponses.Failed, null, $"Unable to create user: {string.Join(" ", createResult.Errors.Select(c => c.Description))}");

        }

        public async Task<CustomResponse<bool>> UpdateUser(User user)
        {
            if (user == null)
                return new CustomResponse<bool>(ServiceResponses.BadRequest, false, "User cannot be null");

            if ((await UserManager.UpdateAsync(user)).Succeeded)
                return new CustomResponse<bool>(ServiceResponses.Success, true, "User Updated Succesfully");

            return new CustomResponse<bool>(ServiceResponses.Failed, false, "Unable to update user");
        }

        public async Task<CustomResponse<GetUserDto>> GetUserById(string id, CancellationToken token)
        {
            var user = await ListAll()

                .FirstOrDefaultAsync(c => c.Id == id, token);

            if (user is null)
                return new CustomResponse<GetUserDto>(ServiceResponses.NotFound, null, "User Not Found");

            return new CustomResponse<GetUserDto>(ServiceResponses.Success, mapper.Map<GetUserDto>(user), null);
        }

        public IQueryable<User> ListAll()
        {
            return UserManager.Users;
        }

        public async Task<CustomResponse<bool>> Delete(User user)
        {
            if (user == null)  
                return new CustomResponse<bool>(ServiceResponses.BadRequest, false, "User cannot be null");

            if((await UserManager.DeleteAsync(user)).Succeeded)  
                return new CustomResponse<bool>(ServiceResponses.Success, true, "User deleted successfully");

            return new CustomResponse<bool>(ServiceResponses.Failed, false, "Unable to delete user");
        }

        public async ValueTask<CustomResponse<AuthResponse>> Login(LoginRequestModel request)
        {
            var existingUser = await UserManager.FindByEmailAsync(request.UserName);
            if(existingUser is null)
            {
                return new CustomResponse<AuthResponse>(ServiceResponses.NotFound, null, "User account not found");
            }

            if (await UserManager.CheckPasswordAsync(existingUser, request.Password))
            {
                var result = await GenerateToken(existingUser);
                return new CustomResponse<AuthResponse>(ServiceResponses.Success, result, null);
            }

            return new CustomResponse<AuthResponse>(ServiceResponses.Failed, null, "Login failed for user");
        }

        private async Task<AuthResponse> GenerateToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_jwt.SigningKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
            };

            var roles = await UserManager.GetRolesAsync(user);
            
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = CreateToken(claims);
            var createdToken = tokenHandler.CreateToken(token);
            _ = int.TryParse(_jwt.RefreshTokenValidityInDays, out int refreshTokenValidityInDays);

            return new AuthResponse()
            {
                ExpiresAt = createdToken.ValidTo,
                UserId = user.Id,
                Token = tokenHandler.WriteToken(createdToken)
            };
        }

        private SecurityTokenDescriptor CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
            _ = int.TryParse(_jwt.TokenValidityInMinutes, out int tokenValidityInMinutes);

            var jwtToken = new SecurityTokenDescriptor()
            {
                Audience = _jwt.Audience,
                Issuer = _jwt.Issuer,
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes),
                IssuedAt = DateTime.UtcNow,
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
            };
            return jwtToken;
        }

    }
}
