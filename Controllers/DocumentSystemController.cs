using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [Route("tree/{Id?}")]
        public async Task<ActionResult<FolderDTO>> GetFolderTree(
                [FromQuery] Guid UserId,
                Guid? Id = null) {
            User user = await m_context.Users.Where(u => u.Id.Equals(UserId)).SingleOrDefaultAsync();
            if (user == null) {
                return StatusCode((int)401, new {
                    ErrorMessage="Not logged in"
                });
            }
            ServiceResponse<List<NodeDTO>> result = await m_docserv.GetFolderTree(Id, user);

            if (result.Success) {
                return Ok(result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                    ErrorMessage = result.ErrorMessage
                });
            }
        }

        [HttpPost]
        [Route("document/upload/{Id}")]
        public async Task<ActionResult<DocumentDTO>> PostDocument(
                Guid UserId, IFormFile document, Guid Id) {
            User user = await m_context.Users.Where(u => u.Id.Equals(UserId)).SingleOrDefaultAsync();
            if (user == null) {
                return StatusCode((int)401, new {
                    ErrorMessage="Not logged in"
                });
            }
            ServiceResponse<DocumentDTO> result = 
                await m_docserv.CreateDocument(Id, document, user);

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
