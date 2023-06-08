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
        public List<NodeDTO> Contents {get; set;} = new List<NodeDTO>();
        public List<PermissionDTO> Permissions {get; set;} = 
            new List<PermissionDTO>();
    }


    public class DocumentDTO : NodeDTO {
        public DocumentDTO() {}
        public DocumentDTO(Guid id, string name) : base(id, name) {}
        public List<PermissionDTO> Permissions {get; set;}
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

    public class PermissionDTO {
        public Guid Id {get; set;}
        public RoleDTO? Role {get; set;}
        public UserDTO? User {get; set;}
        public PermissionMode Mode {get; set;}
    }

    public class RoleDTO {
        public Guid Id {get; set;}
        public string Name {get; set;}
    }

    public class UserDTO {
        public Guid Id {get; set;}
        public string Name {get; set;}
    }

    public class SearchCriteriaDTO {
        public string SearchTerm {get; set;}
    }

    public class SearchResultDTO {
        public NodeDTO Node {get; set;}
        public string Location {get; set;}
    }

    public class MoveNodeDTO {
        public Guid Destination {get; set;}
        public string Name {get; set;}
    }

    public class TreeNodeDTO {
        public Guid Id {get; set;}
        public string Name {get; set;}
        public List<PermissionDTO> Permissions {get; set;} = new List<PermissionDTO>();
        public string Owner {get; set;}
        public List<TreeNodeDTO>? Contents {get; set;}
        public DateTime? Created {get; set;}
        public DateTime? Updated {get; set;}
    }        

    
    public class AddPermissionDTO {
        public Guid? RoleId {get; set;}
        public Guid? UserId {get; set;}
        public PermissionMode Mode {get; set;}
    }
}
