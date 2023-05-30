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
                result.Success = false;
                result.StatusCode = (HttpStatusCode)404;
                result.Data = null;
                result.ErrorMessage = "Requested folder not found";
                return result;
            }

            Folder folder = await m_context.Folders.Include(f => f.Contents)
                .Where(f => f.Id == Id).SingleAsync();

            //Check if user is permitted to read folder
            if (!folder.HasPermission(user, PermissionMode.Read)) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)403;
                result.Data = null;
                result.ErrorMessage = "User does not have permission to " + 
                                      "read requested folder";
                return result;
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
        private async Task<List<NodeDTO>> TraverseFolderTree(Folder folder, User user) {
            List<NodeDTO> contents = new List<NodeDTO>();
            if (folder.HasPermission(user, PermissionMode.Read)) {
                foreach (Node node in folder.Contents) {
                    if (node is Folder) {
                        //Console.WriteLine(node.Name);
                        contents.Add(new FolderDTO() {
                                Id = node.Id,
                                Name = node.Name,
                                Contents = await TraverseFolderTree((Folder)node, user)
                        });
                    } else if (node is Document) {
                        //Console.WriteLine(node.Name);
                        contents.Add(new DocumentDTO() {
                                Id = node.Id, 
                                Name = node.Name
                        });
                    }
                }
            }
            return contents;
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
                result.Success = false;
                result.StatusCode = (HttpStatusCode)404;
                result.ErrorMessage = "Document not found.";
                return result;
            }

            Revision latestRev = document.Revisions.OrderByDescending(
                    r => r.Created).FirstOrDefault();

            if (!latestRev.HasPermission(user, PermissionMode.Read)) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)403;
                result.ErrorMessage = "User does not have permission to read "
                    + "requested document.";
                return result;
            }
            
            Revision? requestedRev;
            if (RevId != null) {
                requestedRev = document.Revisions.Where(
                        r => r.Id == RevId).SingleOrDefault();
                if (requestedRev == null) {
                    result.Success = false;
                    result.StatusCode = (HttpStatusCode)404;
                    result.ErrorMessage = "Requested document revision was "
                        + "not found.";
                    return result;
                }
                if (!requestedRev.HasPermission(user, PermissionMode.Read)) {
                    result.Success = false;
                    result.StatusCode = (HttpStatusCode)403;
                    result.ErrorMessage = "User does not have permission to "
                        + "read requested document revision.";
                    return result;
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
                Guid Id, IFormFile document, User user) {
            ServiceResponse<DocumentDTO> result = 
                new ServiceResponse<DocumentDTO>();

            Folder folder = await m_context.Folders.Include(f => f.Contents)
                .Where(f => f.Id == Id).SingleAsync();

            //Check if user is permitted to write to folder
            if (!folder.HasPermission(user, PermissionMode.Write)) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)403;
                result.Data = null;
                result.ErrorMessage = "User does not have permission to " + 
                                      "write to requested folder";
                return result;
            }

            Document newDocument = new Document {
                Name = document.FileName,
                Owner = user,
                Parent = folder
            };

            m_context.Documents.Add(newDocument);

            Revision revision = new Revision();
            revision.Created = DateTime.Now;
            revision.FileId = Guid.NewGuid();

            newDocument.Revisions.Add(revision);
            
            await m_context.SaveChangesAsync();

            await m_fileservice.StoreFile(revision.FileId.ToString(), document);
            
            DocumentDTO createdDoc = new DocumentDTO(
                    newDocument.Id, newDocument.Name
            );
            result.Success = true;
            result.StatusCode = (HttpStatusCode)201;
            result.Data = createdDoc;
            return result;
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
