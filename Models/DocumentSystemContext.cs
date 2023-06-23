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
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Folder>()
                .HasMany(f => f.Contents)
                .WithOne(n => n.Parent as Folder)
                .HasForeignKey(n => n.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Revision>()
                .HasOne(r => r.Document)
                .WithMany(d => d.Revisions)
                .HasForeignKey(r => r.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Folder>()
                .HasMany(f => f.Permissions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<Revision>()
                .HasMany(r => r.Permissions)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<User>()
                .HasIndex(u => u.Name)
                .IsUnique();
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
            context.SaveChanges();

            
            var users = new User[] {
                new User{Name="admin"},
                new User{Name="user1"}
            };

            foreach(User u in users) {
                u.SetPassword("12345");
                u.Roles.Add(context.Roles.Where(r => r
                            .Name.Equals("Anyone")).Single());
                context.Users.Add(u);
            }

            context.SaveChanges();

            Folder dr = new Folder{Name = "DocumentRoot", Owner = users[0]};
            dr.Permissions.Add(new Permission{Role = context.Roles
                    .Where(r => r.Name.Equals("Anyone")).Single(), 
                    Mode = PermissionMode.Read | PermissionMode.Write});
            context.Folders.Add(dr);
            context.SaveChanges();
            return;
        }
    }
}

