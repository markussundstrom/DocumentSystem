using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using DocumentSystem.Services;
using DocumentSystem.Models;

namespace DocumentSystem.Controllers
{
    [Route("api")]
    [ApiController]
    public class DocumentSystemController : ControllerBase {
        private readonly DocumentSystemService m_docserv;
        private readonly DocumentSystemContext m_context;

        public DocumentSystemController(DocumentSystemService docserv,
                DocumentSystemContext context) {
            this.m_docserv = docserv;
            this.m_context = context;
        }

        [HttpGet]
        [Route("tree/{id?}")]
        public async Task<ActionResult<FolderDTO>> GetFolderTree(
                [FromBody] Guid UserId,
                Guid? Id = null) {
            User user = m_context.Users.Where(u => u.Id == UserId).SingleOrDefault();
            ServiceResponse<List<NodeDTO>> result = m_docserv.GetFolderTree(Id, user);

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
