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
        private readonly UserService m_userv;

        public DocumentSystemController(DocumentSystemService docserv,
                DocumentSystemContext context, UserService userv) {
            this.m_docserv = docserv;
            this.m_context = context;
            this.m_userv  = userv;
        }

        [HttpGet]
        [Route("tree/{Id?}")]
        public async Task<ActionResult<List<TreeNodeDTO>>> GetFolderTree(
                [FromQuery] Guid UserId,
                Guid? Id = null) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                    ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<List<TreeNodeDTO>> result = 
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
        [Route("document/{Id}/metadata")]
        public async Task<ActionResult> GetDocumentInfo (
                [FromQuery] Guid UserId, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
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
        [Route("document/{Id}/download")]
        public async Task<ActionResult> GetDocument(
                [FromQuery] Guid UserId, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
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
            User? user = await m_userv.GetUser(UserId);
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
            User? user = await m_userv.GetUser(UserId);
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


        [HttpPatch]
        [Route("node/move/{Id}")]
        public async Task<ActionResult<NodeDTO>> PatchFolder(
                [FromQuery] Guid UserId, MoveNodeDTO node, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<NodeDTO> result = 
                await m_docserv.MoveNode(Id, node, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else {
                return StatusCode ((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }


        [HttpPut]
        [Route("document/update/{Id}")]
        public async Task<ActionResult<DocumentDTO>> PutDocument(
                [FromQuery] Guid UserId, IFormFile document, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<DocumentDTO> result =
                await m_docserv.UpdateDocument(Id, document, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }


        [HttpPost]
        [Route("tree/search/{Id}")]
        public async Task<ActionResult<List<SearchResultDTO>>>  SearchTree(
                [FromQuery] Guid UserId, Guid Id, 
                [FromBody] SearchCriteriaDTO query) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new { 
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<List<SearchResultDTO>> result = 
                await m_docserv.SearchTree(Id, query, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }


        [HttpDelete]
        [Route("/node/delete/{Id}")]
        public async Task<ActionResult> DeleteNode(
                [FromQuery] Guid UserId, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<int> result = 
                await m_docserv.DeleteNode(Id, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }


        [HttpGet]
        [Route("node/{Id}/permissions")]
        public async Task<ActionResult<List<PermissionDTO>>> GetPermissions(
                [FromQuery] Guid UserId, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<List<PermissionDTO>> result = 
                await m_docserv.GetPermissions(Id, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }


        [HttpPost]
        [Route("node/{Id}/permission")]
        public async Task<ActionResult<List<PermissionDTO>>> PostPermission(
                [FromQuery] Guid UserId, AddPermissionDTO permission, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<List<PermissionDTO>> result = 
                await m_docserv.AddPermission(Id, permission, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }
    }
}
