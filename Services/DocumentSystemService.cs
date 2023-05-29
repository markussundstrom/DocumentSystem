using System.Net;
using Microsoft.EntityFrameworkCore;

using DocumentSystem.Models;

namespace DocumentSystem.Services

{
    ///<summary>
    ///Class <c>DocumentSystemService</c> receives requests for operations 
    ///to be performed in the Document System.
    ///</summary>
    public class DocumentSystemService {
        private readonly DocumentSystemContext m_context;

        public DocumentSystemService (DocumentSystemContext context) {
            this.m_context = context;
        }

        public async Task<ServiceResponse<List<NodeDTO>>> GetFolderTree(
                Guid? Id, User user) {
            ServiceResponse<List<NodeDTO>> result = 
                    new ServiceResponse<List<NodeDTO>>();

            //Check if folder exists
            if (Id != null && ! await m_context.Folders.AnyAsync( f => f.Id == Id)) {
                result.Success = false;
                result.StatusCode = (HttpStatusCode)404;
                result.Data = null;
                result.ErrorMessage = "Requested folder not found";
                return result;
            }

            Folder folder = await m_context.Folders.Include(f => f.Contents).Where(f => f.Id == Id).SingleAsync();
            //FIXME
            Console.WriteLine(folder.Id);
            Console.WriteLine(user.Id);
            Console.WriteLine((int)PermissionMode.Read);
            Console.WriteLine(folder.Contents);
            foreach (Node n in folder.Contents) {
                Console.WriteLine("c: {0} ", n.Id);
            }
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
