using agroApp.Domain;
using agroApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace agroApp.Infra.Data.Context
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


      

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    // Adicione a linha abaixo se ainda não estiver configurada
        //    optionsBuilder.UseSqlServer("Data Source=sqlserveragroconnect.database.windows.net;Initial Catalog=sql_agroconnect;User ID=kamydados;Password=Pacoca2005.;Connect Timeout=30;Encrypt=True",
        //        b => b.MigrationsAssembly("agroApp.Infra.Data"));
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relacionamento User-Profile (um para um)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId);

            // Relacionamento Post-User (um para muitos)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId);

            // Configuração para BlogPost, se necessário
            // ...
        }

    }
}