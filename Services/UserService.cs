using System.Net;
using System.Text;
using System.Configuration;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using DocumentSystem;
using DocumentSystem.Models;

namespace DocumentSystem.Services
{
    ///<summary>
    ///Class <c>UserService</c> handles requests for user data.
    ///</summary>
    public class UserService {
        private readonly DocumentSystemContext m_context;
        private readonly JWTOptions m_jwtOptions;

        public UserService (
                DocumentSystemContext context, JWTOptions jwtOptions) {
            this.m_context = context;
            this.m_jwtOptions = jwtOptions;
        }

        public async Task<ServiceResponse<AuthResponseDTO>> Authenticate(
                CredentialsDTO credentials) {
            ServiceResponse<AuthResponseDTO> result = 
                new ServiceResponse<AuthResponseDTO>();

            User? user = await m_context.Users
                .Where(u => u.Name == credentials.Username)
                .SingleOrDefaultAsync();

            if (user == null) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)401;
                //FIXME
                result.ErrorMessage = "Invalid username";
                return result;
            }

            if (!user.TryPassword(credentials.Password)) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)401;
                //FIXME
                result.ErrorMessage = "Invalid password";
                return result;
            }

            result.Data = new AuthResponseDTO();
            result.Data.Username = user.Name;
            result.Data.AccessToken = CreateAccessToken(user);
            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            return result;
        }

        
        private string CreateAccessToken(User user) {
            byte[] keyBytes = 
                Encoding.UTF8.GetBytes(m_jwtOptions.SigningKey);
            SymmetricSecurityKey symmetricKey = 
                new SymmetricSecurityKey(keyBytes);

            SigningCredentials signingCredentials =
                new SigningCredentials(symmetricKey,
                        SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>() {
                new Claim("sub", user.Id.ToString()),
                new Claim("name", user.Name),
                new Claim("aud", m_jwtOptions.Audience)
            };

            JwtSecurityToken token = new JwtSecurityToken(
                    issuer: m_jwtOptions.Issuer,
                    audience: m_jwtOptions.Audience,
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(120),
                    signingCredentials: signingCredentials
            );

            string tokenString = new JwtSecurityTokenHandler()
                .WriteToken(token);
            return tokenString; 
        }


        public async Task<User?> GetUser(Guid UserId) {
            User user = m_context.Users.Include(u => u.Roles)
                .Where(u => u.Id == UserId)
                .SingleOrDefault();
            return user;
        }
    }
}
