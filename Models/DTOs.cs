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


    public class DocumentInfoDTO : NodeDTO {
        public List<RevisionDTO> Revisions {get;set;} = new List<RevisionDTO>();
        public MetadataDTO Metadata {get; set;}
    }


    public class RevisionDTO {
        public Guid Id {get; set;}
        public DateTime Created {get; set;}
    }


    public class MetadataDTO {
        public DateTime Created;
        public DateTime? Updated;
    }    
}
