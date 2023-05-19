using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DocumentSystem.Models
{
    public class Node {
        [Key]
        public Guid Id {get; set;}
        public string Name {get; set;}
        public Node? Parent {get; set;}
        public User Owner {get; set;}
    }


    public class Folder : Node {
        [InverseProperty("Parent")]
        public List<Node> Contents {get; set;}
        public List<Permission> Permissions {get; set;}
    }


    public class Document : Node {
        public Metadata Metadata {get; set;}
        public List<Revision> Revision {get; set;}
    }


    public class Revision {
        public Guid Id {get; set;}
        public DateTime Created {get; set;}
        public Node Node {get; set;}
        public List<Permission> Permission {get;set;}
    }


    public class Metadata {
        [Key]
        public Guid Id {get; set;}
        public DateTime Created {get; set;}
        public DateTime Updated {get; set;}
    }


    public class Permission {
        public enum PermissionMode {
            None = 0,
            Write = 2,
            Read = 4
        }
        [Key]
        public Guid Id {get; set;}
        public Role? Role {get; set;}
        public User? User {get; set;}
        public PermissionMode Mode {get; set;} 
    }
}
