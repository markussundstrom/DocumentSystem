using System.Net;
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

    
        public async Task<User?> GetUser(Guid UserId) {
            User user = m_context.Users.Include(u => u.Roles)
                .Where(u => u.Id == UserId)
                .SingleOrDefault();
            return user;
        }
    }
}
