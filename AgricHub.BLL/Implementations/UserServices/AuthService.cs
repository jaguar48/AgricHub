using AgricHub.BLL.Helpers;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Contracts;
using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Response;
using Azure.Core;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestSharp.Authenticators;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace AgricHub.BLL.Implementations.UserServices
{
    public sealed class AuthService : IAuthService
    {
        /*private readonly ILoggerManager _logger;*/
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private ApplicationUser? _user;
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmailConfiguration _emailConfig;
        private readonly IRepository<Consultant> _consultantRepo;


        public AuthService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IConfiguration configuration, EmailConfiguration emailConfig)
        {
            /*_logger = logger;*/
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _emailConfig = emailConfig;
            _consultantRepo = _unitOfWork.GetRepository<Consultant>();
            _configuration = configuration;
        }




        public async Task<bool> SendVerificationEmail(string email, string verificationToken)
        {
            var apiKey = "";
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress("");
            var to = new EmailAddress("");
            var subject = "Account Verification";

            var verificationUrl = $"{_configuration["AppBaseUrl"]}/marketplace/authentication/verify?email={HttpUtility.UrlEncode(email)}&verificationToken={verificationToken}";
            var plainTextContent = $"Please click the following link to verify your account: {verificationUrl}";
            var htmlContent = $"<p>Please click the following link to verify your account: <a href='{verificationUrl}'>{verificationUrl}</a></p>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            return response.IsSuccessStatusCode;
        }

        public async Task<ApplicationUser> VerifyUser(string email, string verificationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null && !user.EmailConfirmed && user.VerificationToken == verificationToken)
            {
                user.EmailConfirmed = true;
                user.VerificationToken = null;
                await _userManager.UpdateAsync(user);

                return user;
            }
            return null;
        }


        public async Task<bool> SendPasswordResetEmail(string email, string resetToken)
        {
            var client = new SendGridClient(_emailConfig.ApiKey);

            var from = new EmailAddress(_emailConfig.SenderEmail);
            var to = new EmailAddress(email);
            var subject = "Password Reset";

            var resetUrl = $"{_configuration["AppBaseUrl"]}/marketplace/authentication/reset-password?email={HttpUtility.UrlEncode(email)}&token={resetToken}";
            var plainTextContent = $"Click the following link to reset your password: {resetUrl}";
            var htmlContent = $"<p>Click the following link to reset your password: <a href='{resetUrl}'>{resetUrl}</a></p>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var response = await client.SendEmailAsync(msg);

            return response.IsSuccessStatusCode;
        }


        public async Task<bool> ResetPassword(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            return result.Succeeded;
        }

        public async Task<ServiceResponse<string>> ValidateUser(UserAuthenticationResponse response)
        {
            _user = await _userManager.FindByNameAsync(response.UserName);

            var result = _user != null && await _userManager.CheckPasswordAsync(_user, response.Password);
            if (!result)
            {
                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Login failed. Wrong username or password."
                };
            }

            var roles = await _userManager.GetRolesAsync(_user);
            var role = roles.FirstOrDefault() ?? "Customer"; // ← guard against empty roles

            return new ServiceResponse<string>
            {
                Success = true,
                Message = "Login successful.",
                Role = role
            };
        }



        public async Task<string> CreateToken()
        {

            /* _logger.LogInfo("Creates the JWT token");*/

            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();


            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);
            return new JwtSecurityTokenHandler().WriteToken(tokenOptions);

        }

     
        private SigningCredentials GetSigningCredentials()
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);
            var secret = new SymmetricSecurityKey(key);
            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        private async Task<List<Claim>> GetClaims()
        {

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, _user.Id.ToString()),
                new Claim(ClaimTypes.Name, _user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, _user.Id.ToString()),

            };

            var roles = await _userManager.GetRolesAsync(_user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var tokenOptions = new JwtSecurityToken
            (
            issuer: jwtSettings["validIssuer"],
            audience: jwtSettings["validAudience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["expires"])),
            signingCredentials: signingCredentials
            );
            return tokenOptions;
        }

        public async Task<AuthenticationResponse> GoogleAuth(string credential)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { _configuration["Authentication:Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
            if (payload == null)
                throw new InvalidOperationException("Invalid Google authentication.");

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                var newUser = new ApplicationUser
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = payload.Email,
                    UserName = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(newUser);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create user: {errors}");
                }

                await _userManager.AddToRoleAsync(newUser, "Consultant");

                var info = new UserLoginInfo("GOOGLE", payload.Subject, "GOOGLE");
                await _userManager.AddLoginAsync(newUser, info);

                var consultant = new Consultant
                {
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    Email = payload.Email,
                    BusinessName = payload.Name,
                    UserId = newUser.Id
                };

                await _consultantRepo.AddAsync(consultant);
                await _unitOfWork.SaveChangesAsync();

                _user = newUser;
                var jwtToken = await GenerateToken();

                return new AuthenticationResponse
                {
                    JwtToken = jwtToken,  // ✅ FIXED - pass the whole object
                    UserType = "Consultant",  // ✅ FIXED - add UserType
                    FullName = $"{newUser.FirstName} {newUser.LastName}",
                    TwoFactor = false,
                    IsExisting = false
                };
            }

            // Existing user
            _user = user;
            var token = await GenerateToken();

            return new AuthenticationResponse
            {
                JwtToken = token,  // ✅ FIXED - pass the whole object
                UserType = "Consultant",  // ✅ FIXED - add UserType
                FullName = $"{user.FirstName} {user.LastName}",
                TwoFactor = false,
                IsExisting = true
            };
        }


        public async Task<JwtToken> GenerateToken()
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims();

            var tokenOptions = GenerateTokenOptions(signingCredentials, claims);

            return new JwtToken
            {
                Token = new JwtSecurityTokenHandler().WriteToken(tokenOptions),
                Issued = tokenOptions.ValidFrom,
                Expires = tokenOptions.ValidTo
            };
        }


    }
}
