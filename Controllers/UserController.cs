using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using DocumentSystem.Services;
using DocumentSystem.Models;

namespace DocumentSystem.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase {
        private readonly UserService m_userv;

        public UserController(UserService userv) {
            this.m_userv = userv;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(CredentialsDTO credentials) {
            ServiceResponse<AuthResponseDTO> result = 
                await m_userv.Authenticate(credentials);
            if (result.Success) {
                return Ok(result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }
    }
}


