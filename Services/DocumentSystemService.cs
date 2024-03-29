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


        ///<summary>
        ///Method <c>GetDocumentroot</c> returns the document root folder
        ///</summary>
        public async Task<ServiceResponse<List<NodeDTO>>> GetDocumentRoot(
                User user) {
            ServiceResponse<List<NodeDTO>> result = 
                new ServiceResponse<List<NodeDTO>>();

            List<Folder> rootFolders = new List<Folder>();
            //FIXME Modify to allow returning several root folders
            rootFolders.Add(await m_context.Folders
                    .Where(f => f.Name.Equals("DocumentRoot"))
                    .SingleOrDefaultAsync());
            
            List<NodeDTO> rootFoldersDto = new List<NodeDTO>();

            foreach (Folder f in rootFolders) {
                //if (f.HasPermission(user, PermissionMode.Read)) {
                    rootFoldersDto.Add(new NodeDTO {
                            Id = f.Id,
                            Name = f.Name
                        }
                    );
                    Console.WriteLine(f.Name);
               // }
            }
            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = rootFoldersDto;
            return result;
        }


        ///<summary>
        ///Method <c>GetFolderTree</c> is used to get a list of the contents
        ///of a folder
        ///</summary>
        public async Task<ServiceResponse<List<TreeNodeDTO>>> GetFolderTree(
                Guid? Id, User user) {
            ServiceResponse<List<TreeNodeDTO>> result = 
                    new ServiceResponse<List<TreeNodeDTO>>();

            //Check if folder exists
            if (Id != null && ! await m_context.Folders.AnyAsync(
                        f => f.Id == Id)) {
                return ErrorResponse(result, 404, "folder");
            }

            Folder folder = await m_context.Folders.Include(f => f.Contents)
                .Include(f => f.Permissions).Where(f => f.Id == Id)
                .SingleAsync();

            //Check if user is permitted to read folder
            if (!folder.HasPermission(user, PermissionMode.Read)) {
                return ErrorResponse(result, 403, "folder");
            }

            //List<NodeDTO> tree = await TraverseFolderTree(folder, user);
            List<TreeNodeDTO> tree = await TraverseFolderTree(folder, user);
            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = tree;
            return result;
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

            Revision latestRev = document.Revisions
                .OrderByDescending(r => r.Created).FirstOrDefault();
            m_context.Entry(latestRev).Collection(r => r.Permissions).Load();

            if (!latestRev.HasPermission(user, PermissionMode.Read)) {
                return ErrorResponse(result, 403, "document");
            }

            DocumentInfoDTO docInfo = new DocumentInfoDTO();
            docInfo.Id = document.Id;
            docInfo.Name = document.Name;
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

            Revision latestRev = document.Revisions
                .OrderByDescending(r => r.Created).FirstOrDefault();
            m_context.Entry(latestRev).Collection(r => r.Permissions).Load();

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
                .Include(f => f.Permissions)
                .Where(f => f.Id == Id).SingleAsync();

            if (folder == null) {
                return ErrorResponse(result, 404, "folder");
            }

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

            Folder? parentFolder = await m_context.Folders
                .Include(f => f.Contents).Include(f => f.Permissions)
                .Where(f => f.Id == Id).SingleOrDefaultAsync();

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

            Folder newFolder = new Folder{Name = name, Parent = parentFolder,
                    Owner = user};
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

            Revision latestRev = document.Revisions
                .OrderByDescending(r => r.Created).FirstOrDefault();
            m_context.Entry(latestRev).Collection(r => r.Permissions).Load();

            if (!latestRev.HasPermission(user, PermissionMode.Write)) {
                return ErrorResponse(result, 403, "document");
            }

            DateTime now = DateTime.Now;

            Revision newRev = await GenerateNewRevision(latestRev);
            newRev.FileId = Guid.NewGuid();
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
        ///Method <c>SearchTree</c> searches folders and documents 
        ///according to a given search critera and returns the result
        ///</summary>
        public async Task<ServiceResponse<List<SearchResultDTO>>> SearchTree(
                Guid Id, SearchCriteriaDTO query, User user) {
            ServiceResponse<List<SearchResultDTO>> result = 
                new ServiceResponse<List<SearchResultDTO>>();

            Folder? folder = await m_context.Folders.Include(f => f.Contents)
                .Include(f => f.Permissions).Where(f => f.Id == Id)
                .SingleOrDefaultAsync();

            if (folder == null) {
                return ErrorResponse(result, 404, "folder");
            }

            if (!folder.HasPermission(user, PermissionMode.Read)) {
                return ErrorResponse(result, 403, "folder");
            }

            List<Node>filteredNodes = await FilterNodes(
                    folder, user, query);

            List<SearchResultDTO> searchResults = new  List<SearchResultDTO>();
            foreach (Node node in filteredNodes) {
                SearchResultDTO resultLine = new SearchResultDTO();
                resultLine.Node = new NodeDTO(node.Id, node.Name);
                List<string> pathElements = new List<string>();
                Folder? parent = node.Parent;
                while (parent != null) {
                    pathElements.Add(parent.Name);
                    parent = parent.Parent;
                }
                pathElements.Reverse();
                string path = string.Join("/", pathElements);
                path = "/" + path;
                resultLine.Location = path;
                searchResults.Add(resultLine);
            }
            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = searchResults;
            return result;
        }


        ///<summary>
        ///Method <c>GetPermissions</c> returns a list of permissions for a node
        ///</summary>
        public async Task<ServiceResponse<List<PermissionDTO>>> GetPermissions(
                Guid Id, User user) {
            ServiceResponse<List<PermissionDTO>> result = 
                new ServiceResponse<List<PermissionDTO>>();
            Node? node = await m_context.Nodes.Where(n => n.Id == Id)
                .SingleOrDefaultAsync();

            if (node == null) {
                return ErrorResponse(result, 404, "node");
            }

            if (node.Parent != null && !node.Parent.HasPermission(
                        user, PermissionMode.Read | PermissionMode.Admin)) {
                return ErrorResponse(result, 403, "parent folder");
            }

            List<PermissionDTO> permissions = new List<PermissionDTO>();
            permissions.AddRange(await CreatePermissionDTOList(node));

            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = permissions;
            return result;
        }


        public async Task<ServiceResponse<List<PermissionDTO>>> AddPermission(
                Guid Id, AddPermissionDTO permission, User user) {
            ServiceResponse<List<PermissionDTO>> result =
                new ServiceResponse<List<PermissionDTO>>();

            Node? node = await m_context.Nodes.Where(n => n.Id == Id)
                .SingleOrDefaultAsync();

            if (node == null) {
                return ErrorResponse(result, 404, "node");
            }

            Permission newPermission = new Permission() {
                Mode = permission.Mode
            };
            
            if (permission.RoleId != null) {
                Role? targetRole = await m_context.Roles.Where(
                        r => r.Id == permission.RoleId).SingleOrDefaultAsync();
                newPermission.Role = targetRole;
            }

            if (permission.UserId != null) {
                User? targetUser = await m_context.Users.Where(
                        u => u.Id == permission.UserId).SingleOrDefaultAsync();
                newPermission.User = targetUser;
            }

            if (newPermission.User == null && newPermission.Role == null) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)400;
                result.ErrorMessage = "Either User or Role Id needs to be set";
                return result;
            }

            if (node is Folder folder) {
                if (!folder.HasPermission(user, PermissionMode.Admin)) {
                    return ErrorResponse(result, 403, "folder");
                }
                folder.Permissions.Add(newPermission);
            } else if (node is Document document) {
                Revision latestRev = document.Revisions.OrderByDescending(
                        d => d.Created).First();
                m_context.Entry(latestRev).Collection(
                        r => r.Permissions).Load();
                if(!latestRev.HasPermission(user, PermissionMode.Admin)) {
                    return ErrorResponse(result, 403, "document");
                }
                latestRev.Permissions.Add(newPermission);
            }
            await m_context.SaveChangesAsync();
            
            result.Success = true;
            result.Data = await CreatePermissionDTOList(node);
            result.StatusCode = (HttpStatusCode)201;
            return result;
        }


        ///<summary>
        ///Method <c>ModifyPermission</c> changes the permission mode value
        ///of a given permission.
        ///</summary>
        public async Task<ServiceResponse<PermissionDTO>> 
            ModifyOrDeletePermission(
                    Guid Id, PermissionMode? mode, bool isDelete, User user) {
            ServiceResponse<PermissionDTO> result = 
                new ServiceResponse<PermissionDTO>();

            if (!isDelete && mode == null) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)400;
                result.ErrorMessage = "A permission mode needs to be entered"
                    + "when modifying permission.";
                return result;
            }

            Permission? permission = await m_context.Permissions
                .Where(p => p.Id == Id).SingleOrDefaultAsync();
            
            if (permission == null) {
                return ErrorResponse(result, 404, "permission entry");
            }

            Revision? revision = await m_context.Revisions
                .Where(r => r.Permissions.Any(p => p.Id == Id))
                .FirstOrDefaultAsync();

            Folder? folder = await m_context.Folders
                .Where(f => f.Permissions.Any(p => p.Id == Id))
                .FirstOrDefaultAsync();

            if (revision != null) {
                //Check that the requested permission belongs to the 
                //current document revision
                Document document = revision.Document;
                m_context.Entry(document).Collection(d => d.Revisions).Load();
                Revision latestRev = document.Revisions
                    .OrderByDescending(r => r.Created).First();
                if (latestRev != revision) {
                    result.Success = false;
                    result.StatusCode = (HttpStatusCode)400;
                    result.ErrorMessage = "Requested permission entry is for"
                        + "archived document revision";
                    return result;
                }
                if (!revision.HasPermission(user, PermissionMode.Admin)) {
                        return ErrorResponse(result, 403, "document");
                }

                DateTime now = DateTime.Now;

                Revision newRev = new Revision {
                    Id = Guid.NewGuid(),
                    Created = now,
                    DocumentId = revision.DocumentId,
                    FileId = revision.FileId
                };

                foreach (Permission p in revision.Permissions) {
                    Permission newPerm = new Permission {
                        Id = Guid.NewGuid(),
                        Role = p.Role,
                        User = p.User,
                        Mode = p.Mode
                    };
                    if (p.Id == permission.Id) {
                        if (isDelete) {
                            continue;
                        }
                        newPerm.Mode = (PermissionMode)mode;
                        permission = newPerm;
                    }
                    newRev.Permissions.Add(newPerm);
                }

                document.Revisions.Add(newRev);
                document.Metadata.Updated = now;
            } else if (folder != null) {
                if (!folder.HasPermission(user, PermissionMode.Admin)) {
                        return ErrorResponse(result, 403, "folder");
                }
                if (isDelete) {
                    folder.Permissions.Remove(permission);
                } else {
                    permission.Mode = (PermissionMode)mode;
                }
            } else {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)400;
                result.ErrorMessage = "Permission entry is not attached to"
                    + "either a folder or document revision.";
                return result;
            }

            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            if (!isDelete) {
                m_context.Entry(permission).Reload();
                RoleDTO roleDto = new RoleDTO {
                    Id = permission.Role.Id,
                    Name = permission.Role.Name
                };
                UserDTO userDto = new UserDTO {
                    Id = permission.User.Id,
                    Name = permission.User.Name
                };
                PermissionDTO permissionDto = new PermissionDTO {
                    Id = permission.Id,
                    Role = roleDto,
                    User = userDto,
                    Mode = permission.Mode
                };
                result.Data = permissionDto;
            }
            return result;
        }


        ///<summary>
        ///Method <c>TraverseFolderTree<c> Traverses a folder tree recursively
        ///</summary>
        ///<returns>A List of NodeDTO:s representing the content.</returns>
        private async Task<List<TreeNodeDTO>> TraverseFolderTree(
                Folder folder, User user) {

            List<TreeNodeDTO> contents = new List<TreeNodeDTO>();
            m_context.Entry(folder).Collection(f => f.Contents).Load();

            if (folder.HasPermission(user, PermissionMode.Read)) {
                foreach (Node node in folder.Contents) {
                    TreeNodeDTO treeNodeDto = new TreeNodeDTO();
                    treeNodeDto.Name = node.Name;
                    treeNodeDto.Id = node.Id;
                    treeNodeDto.Owner = node.Owner.Name;
                    if (node is Folder currentFolder) {
                        treeNodeDto.Contents = new List<TreeNodeDTO>();
                        m_context.Entry(currentFolder).Collection(
                                f => f.Permissions).Load();
                        m_context.Entry(currentFolder).Collection(
                                f => f.Contents).Load();
                        treeNodeDto.Contents.AddRange(await TraverseFolderTree(
                                    currentFolder, user));
                    } else if (node is Document currentDocument) {
                        treeNodeDto.Created = currentDocument.Metadata.Created;
                        treeNodeDto.Updated = currentDocument.Metadata.Updated;
                    }
                    treeNodeDto.Permissions = await CreatePermissionDTOList(node);
                    contents.Add(treeNodeDto);
                }
            }
            return contents;
        }


        ///<summary>
        ///Method <c>CreatePermissionDTOList<c> builds a list of permissions for
        ///a given node
        ///</summary>
        private async Task<List<PermissionDTO>> CreatePermissionDTOList(Node node) {
            List<PermissionDTO> dtoPerm = new List<PermissionDTO>();
            List<Permission> permissions = new List<Permission>();
            if (node is Folder folder) {
                permissions.AddRange(folder.Permissions);
            } else if (node is Document document) {
                m_context.Entry(document).Collection(d => d.Revisions).Load();
                Revision latestRev = document.Revisions.OrderByDescending(
                        d => d.Created).First();
                m_context.Entry(latestRev).Collection(
                        r => r.Permissions).Load();
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
        ///Method <c>FilterNodes</c> filters a folder tree according to 
        ///a search query and returns aa list of matches
        ///</summary>
        private async Task<List<Node>> FilterNodes(
                Folder folder, User user, SearchCriteriaDTO query) {
            List<Node> matches = new List<Node>();
            if (folder.HasPermission(user, PermissionMode.Read)) {
                foreach (Node node in folder.Contents) {
                    if (node.Name.Contains(query.SearchTerm)) {
                            matches.Add(node);
                    }

                    if (node is Folder f && folder.Contents != null) {
                        List<Node> subMatches = await FilterNodes(
                                f, user, query);
                        matches.AddRange(subMatches);
                    }
                }
            }
            return matches;
        }

        ///<summary>
        ///Method <c>GenerateNewRevision</c> is used when a new revision for a 
        ///document needs to be generated, for example when updating the 
        ///content or permissions. A revision is supplied as argument, and a 
        ///new revision in whic the desired may be performed is returned.
        ///</summary>
        private async Task<Revision> GenerateNewRevision(Revision baseRev) {
            Revision newRevision = new Revision {
                Id = Guid.NewGuid(),
                Created = DateTime.Now,
                Document = baseRev.Document,
                FileId = baseRev.FileId
            };
            foreach (Permission p in baseRev.Permissions) {
                Permission newP = new Permission {
                    Id = Guid.NewGuid(),
                    Role = p.Role,
                    User = p.User,
                    Mode = p.Mode
                };
                newRevision.Permissions.Add(newP);
            }
            return newRevision;
        }


        ///<summary>
        ///Method <c>MoveNode</c> moves or renames a node, according to the
        ///supplied MoveNodeDTO
        ///</summary>
        public async Task<ServiceResponse<NodeDTO>> MoveNode(
                Guid Id, MoveNodeDTO moveOptions, User user) {
            ServiceResponse<NodeDTO> result =
                new ServiceResponse<NodeDTO>();

            Node? node = m_context.Nodes.Where(n => n.Id == Id)
                .SingleOrDefault();

            if (node == null) {
                return ErrorResponse(result, 404, "object");
            }
            
            if (node is Folder folder) {
                if (!folder.HasPermission(user, PermissionMode.Write)) {
                    return ErrorResponse(result, 403, "folder");
                }
            }else if (node is Document document) {
                Revision latestRev = document.Revisions
                    .OrderByDescending(r => r.Created).First();
                if (!latestRev.HasPermission(user, PermissionMode.Write)) {
                    return ErrorResponse(result, 403, "document");
                }
            }

            if (!node.Parent.HasPermission(user, PermissionMode.Write)) {
                return ErrorResponse(result, 403, "folder");
            }

            if (moveOptions.Destination != null) {
                Folder destFolder = m_context.Folders
                    .Where(f => f.Id == moveOptions.Destination)
                    .SingleOrDefault();
                if (destFolder == null) {
                    return ErrorResponse(result, 404, "destination folder");
                }
                if (!destFolder.HasPermission(user, PermissionMode.Write)) {
                    return ErrorResponse(result, 403, "destination folder");
                }
                node.Parent = destFolder;
            }

            if (moveOptions.Name != null) {
                node.Name = moveOptions.Name;
            }

            NodeDTO resultNode = new NodeDTO(node.Id, node.Name);

            await m_context.SaveChangesAsync();

            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
            result.Data = resultNode;
            return result;
        }


        ///<summary>
        ///Method <c>DeleteNode</c> deletes requested node
        ///</summary>
        public async Task<ServiceResponse<int>> DeleteNode(Guid Id, User user) {
            ServiceResponse<int> result = new ServiceResponse<int>();

            Node node = m_context.Nodes.Where(n => n.Id == Id)
                .SingleOrDefault();
            
            if (node == null) {
                return ErrorResponse(result, 404, "object");
            }

            if (node is Folder folder) {
                if (folder.HasPermission(user, PermissionMode.Write)) {
                    return ErrorResponse(result, 403, "folder");
                }
            }else if (node is Document document) {
                Revision latestRev = document.Revisions
                    .OrderByDescending(r => r.Created).First();
                if (!latestRev.HasPermission(user, PermissionMode.Write)) {
                    return ErrorResponse(result, 403, "document");
                }
            }

            if (!node.Parent.HasPermission(user, PermissionMode.Write)) {
                return ErrorResponse(result, 403, "object");
            }

            m_context.Nodes.Remove(node);

            await m_context.SaveChangesAsync();

            result.Success = true;
            result.StatusCode = (HttpStatusCode)200;
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


}
