namespace DocumentSystem.Models
{
    public class NodeDTO {
        public Guid Id {get; set;}
        public string Name {get; set;}
        public NodeDTO() {}
        public NodeDTO(Guid id, string name) {
            this.Id = id;
            this.Name = name;
        }
    }


    public class FolderDTO : NodeDTO {
        public List<NodeDTO> Contents {get; set;}
    }


    public class DocumentDTO : NodeDTO {
        public DocumentDTO() {}
        public DocumentDTO(Guid id, string name) : base(id, name) {}
    }
}
