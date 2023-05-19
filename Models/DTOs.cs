namespace DocumentSystem.Models
{
    public class NodeDTO {
        public Guid Id {get; set;}
        public string Name {get; set;}
    }


    public class FolderDTO : NodeDTO {
        public List<NodeDTO> Contents {get; set;}
    }


    public class DocumentDTO : NodeDTO {
    }
}
