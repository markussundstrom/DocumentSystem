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
            modelBuilder
                .Entity<Node>()
                .HasOne(n => n.Parent)
                .WithMany()
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Folder>()
                .HasMany(f => f.Contents)
                .WithOne(n => n.Parent as Folder)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Revision>()
                .HasOne(r => r.Document)
                .WithMany(d => d.Revisions)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
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
        public static async void Seed(DocumentSystemContext context) {
            Console.WriteLine("Seed");
            context.Database.EnsureCreated();

            if (context.Nodes.Any()) {
                return;
            }

            context.Roles.Add(new Role{Name="Anyone"});
            context.Roles.Add(new Role{Name="office"});
            context.Roles.Add(new Role{Name="management"});
            //Role role1 = new Role{Name="management"};
            //Role role2 = new Role{Name="office"};
            //context.Roles.Add(role1);
            //context.Roles.Add(role2);
            context.SaveChanges();

            
            var users = new User[] {
                new User{Name="user1",Password="12345"},
                new User{Name="user2",Password="12345"}
            };
            foreach (Role r in context.Roles) {
                Console.WriteLine(r.Name);
            }

            users[0].Roles.Add(context.Roles.Where(r => r.Name.Equals("management")).Single());
            users[1].Roles.Add(context.Roles.Where(r => r.Name.Equals("office")).Single());

            foreach(User u in users) {
                u.Roles.Add(context.Roles.Where(r => r.Name.Equals("Anyone")).Single());
                context.Users.Add(u);
            }

            context.SaveChanges();

            var folders = new Folder[] {
                new Folder{Name="folder1", Owner=users[0]},
                new Folder{Name="folder2", Owner=users[1]}
            };

            foreach(Folder f in folders) {
                context.Folders.Add(f);
            }

            context.Folders.Add(new Folder{Name="folder3", Owner=users[0], Parent=folders[0]});

            context.SaveChanges();

            var documents = new Document[] {
                new Document{Name="doc1", Owner=users[1]},
                new Document{Name="doc2", Owner=users[0], Parent=folders[0]},
                new Document{Name="doc3", Owner=users[0]}
            };

            documents[2].Parent = context.Folders.Where(f => f.Name.Equals("folder3")).SingleOrDefault();
            foreach (Document d in documents) {
                context.Documents.Add(d);
            }
            context.SaveChanges();

            foreach (Document d in context.Documents.ToList()) {
                Console.WriteLine(d.Name);
                Revision r = new Revision{Created=DateTime.Now};
                d.Revisions.Add(r);
                d.Revisions[0].Permissions.Add(new Permission{Role = context.Roles.Where(r => r.Name.Equals("Anyone")).SingleOrDefault(), Mode = PermissionMode.Read | PermissionMode.Write});
            }
            context.SaveChanges();
            return;
        }
    }
}

