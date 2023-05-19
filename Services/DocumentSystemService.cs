namespace DocumentSystem.Services
{
    public class DocumentSystemService {
        public async ServiceResponse<FolderDTO> FolderTree(Guid Id) {
            //Folder exists
            //Permission to folder
        }
    }


    public class ServiceResponse<T> {
        public HttpStatusCode StatusCode {get; set;}
        public T Data {get; set;}
        public String ErrorMessage {get; set;}
    }
}
