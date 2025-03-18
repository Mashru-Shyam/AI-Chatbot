using AI_Chatbot.Interfaces;
using AI_Chatbot.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AI_Chatbot.Services
{
    public class JwtService : IJwtService
    {
        private readonly string secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? throw new InvalidOperationException("JWT_KEY environment variable is not set.");
        private readonly string issuer = Environment.GetEnvironmentVariable("CHATBOT_PROJECT") ?? throw new InvalidOperationException("Issuervenvironment variable is not set."); 
        private readonly string audience = Environment.GetEnvironmentVariable("CHATBOT_PROJECT") ?? throw new InvalidOperationException("Audience environment variable is not set.");

        //Retrive a JWT Token
        public string GetToken(int userId)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT SecretKey is not configured.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
