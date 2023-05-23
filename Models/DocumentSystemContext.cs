using Microsoft.EntityFrameworkCore;

namespace DocumentSystem.Models
{
    public class DocumentSystemContext : DbContext {
        public DocumentSystemContext(
            DbContextOptions<DocumentSystemContext> options
        ) : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            //new DbInitializer().Seed(this);
        }

        public DbSet<Node> Nodes {get; set;} = null!;
        public DbSet<Folder> Folders {get; set;} = null!;
        public DbSet<Document> Documents {get; set;} = null!;
        public DbSet<Metadata> Metadata {get; set;} = null!;
        public DbSet<Permission> Permissions {get; set;} = null!;
        public DbSet<User> Users {get; set;} = null!;
        public DbSet<Role> Roles {get; set;} = null!;
        public DbSet<Revision> Revisions {get; set;} = null!;
    }


    public static class DbInitializer {
        public static void Seed(DocumentSystemContext context) {
            Console.WriteLine("Seed");
            context.Database.EnsureCreated();

            if (context.Nodes.Any()) {
                return;
            }

            context.Roles.Add(new Role{Name="Anyone"});
            context.Roles.Add(new Role{Name="office"});
            context.Roles.Add(new Role{Name="management"});
            context.SaveChanges();
            
            var users = new User[] {
                new User{Name="user1",Password="12345"},
                new User{Name="user2",Password="12345"}
            };
            users[0].Roles.Add(context.Roles.Where(r => r.Name.Equals("management")).SingleOrDefault());
            users[1].Roles.Add(context.Roles.Where(r => r.Name.Equals("office")).SingleOrDefault());
            foreach(User u in users) {
                u.Roles.Add(context.Roles.Where(r => r.Name.Equals("Anyone")).SingleOrDefault());
                context.Users.Add(u);
            }
            context.SaveChanges();

            var folders = new Folder[] {
                new Folder{Name="folder1"},
                new Folder{Name="folder2"}
            };

            foreach(Folder f in folders) {
                context.Folders.Add(f);
            }
            context.SaveChanges();

            context.Folders.Add(new Folder{Name="folder3",Parent=context.Folders.Where(f => f.Name.Equals("folder1")).SingleOrDefault()});
            context.SaveChanges();

            var documents = new Document[] {
                new Document{Name="doc1"},
                new Document{Name="doc2"},
                new Document{Name="doc3"}
            };

            documents[1].Parent = context.Folders.Where(f => f.Name.Equals("folder2")).SingleOrDefault();
            documents[2].Parent = context.Folders.Where(f => f.Name.Equals("folder3")).SingleOrDefault();

            foreach(Document d in documents) {
                d.Revisions.Add(new Revision{Created = DateTime.Now});
                d.Revisions[0].Permissions.Add(new Permission{Role = context.Roles.Where(r => r.Name.Equals("Anyone")).SingleOrDefault(), Mode = PermissionMode.Read | PermissionMode.Write});
                context.Documents.Add(d);
            }
            context.SaveChanges();
            return;
        }
    }
}
