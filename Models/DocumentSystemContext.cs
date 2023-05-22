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
            new DbInitializer(modelBuilder).Seed();
        }

        public DbSet<Node> Nodes {get; set;} = null!;
        public DbSet<Folder> Folders {get; set;} = null!;
        public DbSet<Document> Documents {get; set;} = null!;
        public DbSet<Metadata> Metadata {get; set;} = null!;
        public DbSet<Permission> Permissions {get; set;} = null!;
        public DbSet<User> Users {get; set;} = null!;
        public DbSet<Role> Roles {get; set;} = null!;
    }


    public class DbInitializer {
        private readonly ModelBuilder modelBuilder;

        public DbInitializer(ModelBuilder modelBuilder) {
            this.modelBuilder = modelBuilder;
        }

        public void Seed() {
/*            modelBuilder.Entity<Role>().HasData(
                new Role(){Id = 1, Name = "anyone"}
            );
            modelBuilder.Entity<User>().HasData(
                new User(){Id = 1, Name = "Admin", Password = "Admin"}
            );
       */ }
    }
                
}
