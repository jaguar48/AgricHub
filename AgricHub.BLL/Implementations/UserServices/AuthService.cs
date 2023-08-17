using AgricHub.BLL.Helpers;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
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
        private readonly EmailConfiguration _emailConfig;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration, EmailConfiguration emailConfig)
        {
            /*_logger = logger;*/
            _userManager = userManager;
            _emailConfig = emailConfig;
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
            /* _logger.LogInfo("Validates user and logs them in");*/

            _user = await _userManager.FindByNameAsync(response.UserName);

            var result = _user != null && await _userManager.CheckPasswordAsync(_user, response.Password);
            if (!result)
            {
                /*_logger.LogWarn($"{nameof(ValidateUser)}: Authentication failed. Wrong username or password.");*/

                return new ServiceResponse<string>
                {
                    Success = false,
                    Message = "Login failed. Wrong username or password."
                };
            }

            var role = (await _userManager.GetRolesAsync(_user))[0];
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

        private static SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("SECRET"));
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


    }
}
