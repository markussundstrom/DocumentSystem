using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace DocumentSystem.Models
{
    public class User {
        [Key]
        public Guid Id {get; set;}
        public string Name {get; set;}
        public string Password {get; set;}
        public List<Role> Roles {get; set;}
    }


    public class Role {
        [Key]
        public Guid Id {get; set;}
        public string Name {get; set;}
        public List<User> Users {get; set;}
    }
}
