using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DocumentSystem.Models
{
    public class Node {
        [Key]
        public Guid Id {get; set;}
        public string Name {get; set;}
        public Guid? ParentId {get; set;}
        public Node? Parent {get; set;}
        public User Owner {get; set;}
    }


    public class Folder : Node {
        public List<Node> Contents {get; set;} = new List<Node>();
        public List<Permission> Permissions {get;set;} = new List<Permission>();

        ///<summary>
        ///Method <c>HasPermission<c> checks if a given user has specified
        ///permission to the folder
        ///</summary>
        ///<returns>True if user has permission, otherwise false</returns>
        public bool HasPermission(User user, PermissionMode mode) {
            if (user == this.Owner) {
                return true;
            }

            List<Permission> matchedPerms = new List<Permission>();

            matchedPerms.AddRange(Permissions.Where(p => p.User == user));
             
            matchedPerms.AddRange(Permissions.Where(
                    p => user.Roles.Any(q => q == p.Role)));
                    
            foreach (Permission perm in matchedPerms) {
                if ((mode & perm.Mode) == perm.Mode)  {
                    return true;
                }
            }
            return false;
        }
    }


    public class Document : Node {
        public Metadata Metadata {get; set;} = new Metadata();
        public List<Revision> Revisions {get; set;} = new List<Revision>();
    }


    public class Revision {
        public Guid Id {get; set;}
        public DateTime Created {get; set;}
        public Document Document {get; set;}
        public Guid DocumentId {get; set;}
        public Guid FileId {get; set;}
        public List<Permission> Permissions {get;set;} = new List<Permission>();
    }


    public class Metadata {
        [Key]
        public Guid Id {get; set;}
        public DateTime Created {get; set;}
        public DateTime? Updated {get; set;}

        public Metadata() {
            Created = DateTime.Now;
        }
    }


    public class Permission {
        [Key]
        public Guid Id {get; set;}
        public Role? Role {get; set;}
        public User? User {get; set;}
        public PermissionMode Mode {get; set;} 
    }


    public enum PermissionMode {
        None = 0,
        Write = 2,
        Read = 4
    }
}
