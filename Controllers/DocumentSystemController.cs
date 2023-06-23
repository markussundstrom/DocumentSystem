using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
        ///<summary>
        ///Get the id for the document root folder
        ///</summary>
        [HttpGet]
        [Authorize]
        [Route("documentroot")]
        public async Task<ActionResult<List<NodeDTO>>> GetDocumentRoot() {
            Guid userGuid = new Guid();
            Guid.TryParse(
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    out userGuid
            );
            User? user = await m_userv.GetUser(userGuid);
            if (user == null) {
                return StatusCode((int)401, new {
                    ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<List<NodeDTO>> result = 
                await m_docserv.GetDocumentRoot(user);

            if (result.Success) {
                return Ok(result.Data);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }

        ///<summary>
        ///List the contents of a folder recursively
        ///</summary>
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


        ///<summary>
        ///Get metadata for a document
        ///</summary>
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


        ///<summary>
        ///Download the document content
        ///</summary>
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


        ///<summary>
        ///Upload a file and create a new document
        ///</summary>
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


        ///<summary>
        ///Create a new folder
        ///</summary>
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


        ///<summary>
        ///Rename or move a document or folder
        ///</summary>
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


        ///<summary>
        ///Update a document's content
        ///</summary>
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


        ///<summary>
        ///Search the document storage
        ///</summary>
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


        ///<summary>
        ///Delete a document or folder
        ///</summary>
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


        ///<summary>
        ///View the permissions for a document or folder
        ///</summary>
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


        ///<summary>
        ///Add a new permission entry to a document or folder
        ///</summary>
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


        ///<summary>
        ///Modify the mode of a permission entry
        ///</summary>
        [HttpPatch]
        [Route("permission/{Id}")]
        public async Task<ActionResult<PermissionDTO>> PatchPermission(
                [FromQuery] Guid UserId, PermissionMode mode, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<PermissionDTO> result = 
                await m_docserv.ModifyOrDeletePermission(Id, mode, false, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode, result.Data);
            } else  {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }


        ///<summary>
        ///Delete a permission entry
        ///</summary>
        [HttpDelete]
        [Route("permission/{Id}")]
        public async Task<ActionResult> DeletePermission(
                [FromQuery] Guid UserId, Guid Id) {
            User? user = await m_userv.GetUser(UserId);
            if (user == null) {
                return StatusCode((int)401, new {
                        ErrorMessage="Not logged in"
                });
            }

            ServiceResponse<PermissionDTO> result = 
                await m_docserv.ModifyOrDeletePermission(Id, null, true, user);

            if (result.Success) {
                return StatusCode((int)result.StatusCode);
            } else {
                return StatusCode((int)result.StatusCode, new {
                        ErrorMessage = result.ErrorMessage
                });
            }
        }
    }
}
