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
            User user = await m_context.Users.Where(u => u.Id.Equals(UserId))
                .SingleOrDefaultAsync();
            if (user == null) {
                return StatusCode((int)401, new {
                    ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<List<NodeDTO>> result = 
                await m_docserv.GetFolderTree(Id, user);

            if (result.Success) {
                return Ok(result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                    ErrorMessage = result.ErrorMessage
                });
            }
        }


        [HttpGet]
        [Route("documentinfo/{Id}")]
        public async Task<ActionResult> GetDocumentInfo (
                [FromQuery] Guid UserId, Guid Id) {
            User user = await m_context.Users.Where(u => u.Id.Equals(UserId))
                .SingleOrDefaultAsync();
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage = "Not logged in"
                });
            }

            ServiceResponse<DocumentInfoDTO> result = 
                await m_docserv.GetDocumentInfo(Id, user);

            if (result.Success) {
                return Ok(result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }
        [HttpGet]
        [Route("document/{Id}")]
        public async Task<ActionResult> GetDocument(
                [FromQuery] Guid UserId, Guid Id) {
            User user = await m_context.Users.Where(u => u.Id.Equals(UserId))
                .SingleOrDefaultAsync();
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<FileContentResult> result =
                await m_docserv.RetrieveDocument(Id, user);

            if (result.Success) {
                return result.Data;
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
            User user = await m_context.Users.Where(u => u.Id.Equals(UserId))
                .SingleOrDefaultAsync();
            if (user == null) {
                return StatusCode((int)401, new {
                    ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<DocumentDTO> result = 
                await m_docserv.CreateDocument(Id, document, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                    ErrorMessage = result.ErrorMessage
                });
            }
        }


        [HttpPost]
        [Route("folder/create/{Id}")]
        public async Task<ActionResult<FolderDTO>> PostFolder(
                [FromQuery] Guid UserId, string Name, Guid Id) {
            User user = await m_context.Users.Where(u => u.Id.Equals(UserId)).SingleOrDefaultAsync();
            if (user == null) {
                return StatusCode((int)401, new {
                    ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<FolderDTO> result = 
                await m_docserv.CreateFolder(Id, Name, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else {
                return StatusCode ((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }



    }
}
