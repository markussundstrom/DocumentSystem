namespace DocumentSystem.Services
{
    ///<summary>
    ///Class <C>DocumentSystemService</c> receives requests for operations 
    ///to be performed in the Document System.
    ///</summary>
    public class DocumentSystemService {
        private readonly DocumentSystemContext m_context;

        public DocumentSystemService (DocumentSystemContext context) {
            this.m_context = context;
        }

        public async ServiceResponse<FolderDTO> FolderTree(Guid Id) {
            ServiceResponse<List<NodeDTO>> result = 
                    new ServiceResponse<List<NodeDTO();

            //Check if folder exists
            if (Id != null && m_context.Folders.Where(
                    f => f.Id == Id).AnyAsync()) {
                result.Success = false;
                result.StatusCode = 404;
                result.Data = null;
                result.ErrorMessage = "Requested folder not found";
                return result;
            }

            List<Permission> permissions = new List<Permission>(
                    m_context.Folders.Where(f => f.Id == id).
                    SingleAsync().Permission
            if (HasPermission(permissions, user, PermissionMode.Read)) {
            }
            
                
            //Permission to folder
        }

        public async bool HasPermission(List<Permission>
            if
    }


    ///<summary>
    ///Class <c>ServiceResponse<T></c> is used as return value
    ///from the service layer.
    ///</summary>
    public class ServiceResponse<T> {
        public bool Success {get; set;}
        public HttpStatusCode StatusCode {get; set;}
        public T? Data {get; set;}
        public String ErrorMessage {get; set;}
    }
}
