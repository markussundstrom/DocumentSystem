using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace DocumentSystem.Models
{
    public class User {
        [Key]
        public Guid Id {get; set;}
        public string Name {get; set;}
        public string Password {get; set;}
        public List<Role> Roles {get; set;} = new List<Role>();
        public string Salt {get; set;}


        public bool TryPassword(string password) {
            password += Salt;
            byte[] pwBytes = Encoding.UTF8.GetBytes(Password);
            HashAlgorithm algo = HashAlgorithm.Create("SHA512");
            byte[] hash = algo.ComputeHash(pwBytes);
            return(Password == Convert.ToBase64String(hash));
        }

        public bool SetPassword(string newPassword) {
            string salt = Guid.NewGuid().ToString("N");
            newPassword += salt;
            byte[] pwBytes = Encoding.UTF8.GetBytes(newPassword);
            HashAlgorithm algo = HashAlgorithm.Create("SHA512");
            byte[] hash = algo.ComputeHash(pwBytes);
            Password = Convert.ToBase64String(hash);
            Salt = salt;
            return true;
        }

    }


    public class Role {
        [Key]
        public Guid Id {get; set;}
        public string Name {get; set;}
        [InverseProperty("Roles")]
        public List<User> Users {get; set;} 
    }
}
