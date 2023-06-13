using System.Net;
using System.Text;
using System.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using DocumentSystem.Models;

namespace DocumentSystem.Services
{
    ///<summary>
    ///Class <c>UserService</c> handles requests for user data.
    ///</summary>
    public class UserService {
        private readonly DocumentSystemContext m_context;

        public UserService (DocumentSystemContext context) {
            this.m_context = context;
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
                result.ErrorMessage = "Invalid username or password";
                return result;
            }

            if (!user.TryPassword(credentials.Password)) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)401;
                result.ErrorMessage = "Invalid username or password";
            }

            result.Data = new AuthResponseDTO();
            result.Data.Username = user.Name;
            result.Data.AccessToken = CreateAccessToken(user);
            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            return result;
        }

        
        private string CreateAccessToken(User user) {


            char[] keyBytes = 
                Encoding.UTF8.GetBytes(config.JWT.Key);
                        //["JWT:Key"]);
            SymmerticSecurityKey symmetricKey = 
                new SymmetricSecurityKey(keyBytes);

            SigningCredentials signingCredentials =
                new SigningCredentials(symmetricKey,
                        SecurityAlgorithms.HmacSha256);

            List<Claim> claims = new List<Claim>() {
                new Claim("sub", user.Id),
                new Claim("name", user.Name)
            };

            JwtSecurityToken token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(180)
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
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
