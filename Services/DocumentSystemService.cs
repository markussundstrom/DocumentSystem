using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

using DocumentSystem.Models;

namespace DocumentSystem.Services

{
    ///<summary>
    ///Class <c>DocumentSystemService</c> receives requests for operations 
    ///to be performed in the Document System.
    ///</summary>
    public class DocumentSystemService {
        private readonly DocumentSystemContext m_context;
        private readonly FileService m_fileservice;

        public DocumentSystemService (
                DocumentSystemContext context, 
                FileService fileservice) {
            this.m_context = context;
            this.m_fileservice = fileservice;
        }


        public async Task<ServiceResponse<List<NodeDTO>>> GetFolderTree(
                Guid? Id, User user) {
            ServiceResponse<List<NodeDTO>> result = 
                    new ServiceResponse<List<NodeDTO>>();

            //Check if folder exists
            if (Id != null && ! await m_context.Folders.AnyAsync(
                        f => f.Id == Id)) {
                return ErrorResponse(result, 404, "folder");
            }

            Folder folder = await m_context.Folders.Include(f => f.Contents)
                .Where(f => f.Id == Id).SingleAsync();

            //Check if user is permitted to read folder
            if (!folder.HasPermission(user, PermissionMode.Read)) {
                return ErrorResponse(result, 403, "folder");
            }

            List<NodeDTO> tree = await TraverseFolderTree(folder, user);
            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = tree;
            return result;
        }

        ///<summary>
        ///Method <c>TraverseFolderTree<c> Traverses a folder tree recursively
        ///</summary>
        ///<returns>A FolderDTO containing the folder tree</returns>
        private async Task<List<NodeDTO>> TraverseFolderTree(
                Folder folder, User user) {
            List<NodeDTO> contents = new List<NodeDTO>();
            if (folder.HasPermission(user, PermissionMode.Read)) {
                foreach (Node node in folder.Contents) {
                    if (node is Folder) {
                            
                        contents.Add(new FolderDTO() {
                                Id = node.Id,
                                Name = node.Name,
                                Permissions = await GetPermissionDTOList(node),
                                Contents = await TraverseFolderTree(
                                        (Folder)node, user)
                        });
                    } else if (node is Document) {
                        contents.Add(new DocumentDTO() {
                                Id = node.Id, 
                                Name = node.Name,
                                Permissions = await GetPermissionDTOList(node)
                        });
                    }
                }
            }
            return contents;
        }

        ///<summary>
        ///Method <c>GetPermissionDTOList<c> returns a list of permissions for
        ///a given node
        ///</summary>
        private async Task<List<PermissionDTO>> GetPermissionDTOList(Node node) {
            List<PermissionDTO> dtoPerm = new List<PermissionDTO>();
            List<Permission> permissions = new List<Permission>();
            if (node is Folder folder) {
                permissions.AddRange(folder.Permissions);
            } else if (node is Document document) {
                Revision latestRev = document.Revisions.OrderByDescending(
                        d => d.Created).First();
                permissions.AddRange(latestRev.Permissions);
            }
            foreach (Permission p in permissions) {
                PermissionDTO pd = new PermissionDTO{ Mode = p.Mode};
                if (p.Role != null) {
                    pd.Role = new RoleDTO{Name = p.Role.Name, Id = p.Role.Id};
                } else if (p.User != null) {
                    pd.User = new UserDTO{Name = p.User.Name, Id = p.User.Id};
                }
                dtoPerm.Add(pd);
            }
            return dtoPerm;
        }


        ///<summary>
        ///Method <c>GetDocumentInfo</c>  returns document information such 
        ///as metadata and revision history for a given document.
        ///</summary>
        public async Task<ServiceResponse<DocumentInfoDTO>> GetDocumentInfo(
                Guid Id, User user) {
            ServiceResponse<DocumentInfoDTO> result = 
                new ServiceResponse<DocumentInfoDTO>();

            Document? document = await m_context.Documents.Include(
                    d => d.Revisions).Include(d => d.Metadata).Where(
                    d => d.Id == Id).SingleOrDefaultAsync();

            if (document == null) {
                return ErrorResponse(result, 404, "document");
            }

            Revision latestRev = document.Revisions.OrderByDescending(
                    r => r.Created).FirstOrDefault();

            if (!latestRev.HasPermission(user, PermissionMode.Read)) {
                return ErrorResponse(result, 403, "document");
            }

            DocumentInfoDTO docInfo = new DocumentInfoDTO();
            docInfo.Metadata = new MetadataDTO{
                Created = document.Metadata.Created,
                Updated = document.Metadata.Updated
            };

            foreach (Revision rev in document.Revisions) {
                if (rev.HasPermission(user, PermissionMode.Read)) {
                    docInfo.Revisions.Add(
                        new RevisionDTO{
                            Id = rev.Id,
                            Created = rev.Created
                    });
                }
            }

            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = docInfo;
            return result;
        }


        ///<summary>
        ///Method <c>RetrieveDocument</c> retrieves and returns a given 
        ///document from storage.
        ///</summary>
        public async Task<ServiceResponse<FileContentResult>> RetrieveDocument(
                Guid Id, User user, Guid? RevId = null) {
            ServiceResponse<FileContentResult> result =
                new ServiceResponse<FileContentResult>();

            Document? document = await m_context.Documents.Include(
                    d => d.Revisions).Where(d => d.Id == Id)
                    .SingleOrDefaultAsync();

            if (document == null) {
                return ErrorResponse(result, 404, "document");
            }

            Revision latestRev = document.Revisions.OrderByDescending(
                    r => r.Created).FirstOrDefault();

            if (!latestRev.HasPermission(user, PermissionMode.Read)) {
                return ErrorResponse(result, 403, "document");
            }
            
            Revision? requestedRev;
            if (RevId != null) {
                requestedRev = document.Revisions.Where(
                        r => r.Id == RevId).SingleOrDefault();
                if (requestedRev == null) {
                    return ErrorResponse(result, 404, "document revision");
                }
                if (!requestedRev.HasPermission(user, PermissionMode.Read)) {
                    return ErrorResponse(result, 403, "document revision");
                }
            } else {
                requestedRev = latestRev;
            }

            byte[] fileContents;
            try {
                fileContents = m_fileservice.GetFile(
                        requestedRev.FileId.ToString());
            } catch (Exception e) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)500;
                result.ErrorMessage = e.Message;
                return result;
            }

            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = new FileContentResult(
                    fileContents, "application/octet-stream");
            result.Data.FileDownloadName = document.Name;
            return result;
        }


        ///<summary>
        ///Method <c>CreateDocument</c> saves a new document and returns
        ///the document information.
        ///</summary>
        public async Task<ServiceResponse<DocumentDTO>> CreateDocument(
                Guid Id, IFormFile file, User user) {
            ServiceResponse<DocumentDTO> result = 
                new ServiceResponse<DocumentDTO>();

            Folder folder = await m_context.Folders.Include(f => f.Contents)
                .Where(f => f.Id == Id).SingleAsync();

            //FIXME
            //Folder exists?
            //
            //Check if user is permitted to write to folder
            if (!folder.HasPermission(user, PermissionMode.Write)) {
                return ErrorResponse(result, 403, "folder");
            }

            DateTime now = DateTime.Now;

            Document newDocument = new Document {
                Name = file.FileName,
                Owner = user,
                Parent = folder
            };

            m_context.Documents.Add(newDocument);

            Revision revision = new Revision();
            revision.Created = now;
            revision.FileId = Guid.NewGuid();

            newDocument.Revisions.Add(revision);
            newDocument.Metadata = new Metadata{Created = now};
            
            await m_fileservice.StoreFile(revision.FileId.ToString(), file);
            
            await m_context.SaveChangesAsync();

            DocumentDTO createdDoc = new DocumentDTO(
                    newDocument.Id, newDocument.Name
            );
            result.Success = true;
            result.StatusCode = (HttpStatusCode)201;
            result.Data = createdDoc;
            return result;
        }


        ///<summary>
        ///Method <c>Folder</c> creates a new subfolder within the given folder
        ///</summary>
        public async Task<ServiceResponse<FolderDTO>> CreateFolder(
                Guid Id, string name, User user) {
            ServiceResponse<FolderDTO> result = 
                new ServiceResponse<FolderDTO>();

            Folder? parentFolder = await m_context.Folders.Include(
                    f => f.Contents).Where(f => f.Id == Id)
                    .SingleOrDefaultAsync();

            if (parentFolder == null) {
                return ErrorResponse(result, 404, "folder");
            }

            if (!parentFolder.HasPermission(user, PermissionMode.Write)) {
                return ErrorResponse(result, 403, "folder");
            }

            if (String.IsNullOrEmpty(name)) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)400;
                result.ErrorMessage = "Name for the new folder is required";
                return result;
            }

            Folder newFolder = new Folder{Name = name, Parent = parentFolder};
            m_context.Folders.Add(newFolder);
            await m_context.SaveChangesAsync();

            FolderDTO createdFolder = new FolderDTO {
                Name = newFolder.Name, Contents = new List<NodeDTO>()
            };

            result.Success = true;
            result.StatusCode = (HttpStatusCode)201;
            result.Data = createdFolder;
            return result;
        }


        ///<summary>
        ///Method <c>UpdateDocument</c> recieves a file for an existing
        ///document, and creates a new revision pointing to the new file.
        ///</summary>
        public async Task<ServiceResponse<DocumentDTO>> UpdateDocument(
                Guid Id, IFormFile file, User user) {
            ServiceResponse<DocumentDTO> result = 
                new ServiceResponse<DocumentDTO>();

            Document? document = await m_context.Documents.Include(
                    d => d.Revisions).Include(d => d.Metadata)
                    .Where(d => d.Id == Id).SingleOrDefaultAsync();

            if (document == null) {
                return ErrorResponse(result, 404, "document");
            }

            Revision latestRev = document.Revisions.OrderByDescending(
                r => r.Created).FirstOrDefault();

            if (!latestRev.HasPermission(user, PermissionMode.Write)) {
                return ErrorResponse(result, 403, "document");
            }

            DateTime now = DateTime.Now;

            Revision newRev = new Revision{
                Created = now, FileId = Guid.NewGuid()
            };
            document.Revisions.Add(newRev);
            document.Metadata.Updated = now;

            await m_fileservice.StoreFile(newRev.FileId.ToString(), file);

            await m_context.SaveChangesAsync();

            DocumentDTO modifiedDoc =
                new DocumentDTO(document.Id, document.Name);

            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = modifiedDoc;
            return result;
        }


        ///<summary>
        ///Method <c>ErrorResponse</c> is a helper method that populates
        ///a ServiceResponse<T> object with error information.
        ///</summary>
        private ServiceResponse<T> ErrorResponse<T>(
                ServiceResponse<T> response, int code, string errorObj) {
            response.Success = false;
            response.StatusCode = (HttpStatusCode)code;
            response.ErrorMessage = code switch {
                403 => "User does not have sufficient permission for " +
                       "requested operation on " + errorObj + ".",
                404 => "Unable to find requested " + errorObj + ".",
                _   => "Error"
            };
            return response;
        }
    }


    ///<summary>
    ///Class <c>ServiceResponse<T></c> is used as return value
    ///from the service layer.
    ///</summary>
    public class ServiceResponse<T> {
        public bool Success {get; set;}
        public HttpStatusCode StatusCode {get; set;}
        public T? Data {get; set;}
        public String? ErrorMessage {get; set;}
    }
}
