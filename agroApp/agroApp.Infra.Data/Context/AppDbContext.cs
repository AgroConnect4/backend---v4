using agroApp.Domain;
using agroApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace agroApp.Infra.Data.Context
{
    public class AppDbContext : DbContext
    {
        //User
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Connection> Connections { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Tip> Tips { get; set; }
        public DbSet<Banner> Banners { get; set; }

        public DbSet<BlogPost> BlogPosts { get; set; }

        //Event
        public DbSet<Event> Events { get; set; }
        public DbSet<EventComment> EventComments { get; set; }
        public DbSet<EventShare> EventShares { get; set; }
        public DbSet<EventParticipant> EventParticipants { get; set; }

        //Post
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostReaction> PostReactions { get; set; }
        public DbSet<PostReport> PostReports { get; set; }
        public DbSet<PostComment> PostComments { get; set; }
        public DbSet<PostShare> PostShares { get; set; }

        //Perfil
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Farm> Farms { get; set; }
        public DbSet<Specialization> Specializations { get; set; }
        public DbSet<PortfolioItem> PortfolioItems { get; set; }
        public DbSet<ContactMethod> ContactMethods { get; set; }

        public DbSet<Notification> Notifications { get; set; }
        

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<Connection>()
                .HasKey(c => new { c.UserId, c.ConnectedUserId }); 
                
            modelBuilder.Entity<News>()
                .HasOne(n => n.Author)
                .WithMany(u => u.News) // Corrected navigation property for News
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tip to Author (one-to-many)
            modelBuilder.Entity<Tip>()
                .HasOne(t => t.Author)
                .WithMany(u => u.Tips) // Corrected navigation property for Tips
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Banner to User (one-to-many)
            modelBuilder.Entity<Banner>()
                .HasOne(b => b.User)
                .WithMany(u => u.Banners) // Added navigation property for Banners
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento User-Profile (um para um)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento User-UserRole (um para muitos)
            modelBuilder.Entity<User>()
                .HasMany(u => u.UserRoles)
                .WithOne(ur => ur.User)
                .HasForeignKey(ur => ur.UserId);

                modelBuilder.Entity<Farm>()
                .HasOne(f => f.Profile)
                .WithMany(p => p.Farms)
                .HasForeignKey(f => f.ProfileId);

            modelBuilder.Entity<Specialization>()
                .HasOne(s => s.Profile)
                .WithMany(p => p.Specializations)
                .HasForeignKey(s => s.ProfileId);

            modelBuilder.Entity<PortfolioItem>()
                .HasOne(p => p.Profile)
                .WithMany(p => p.Portfolio)
                .HasForeignKey(p => p.ProfileId);

            modelBuilder.Entity<ContactMethod>()
                .HasOne(cm => cm.Profile) // Link to Profile
                .WithMany(p => p.ContactMethods)
                .HasForeignKey(cm => cm.ProfileId);

             modelBuilder.Entity<Post>()
                .HasMany(p => p.Reactions)
                .WithOne(r => r.Post)
                .HasForeignKey(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Relacionamento Role-UserRole (um para muitos)
            modelBuilder.Entity<Role>()
                .HasMany(r => r.UserRoles)
                .WithOne(ur => ur.Role)
                .HasForeignKey(ur => ur.RoleId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Relacionamento Post-User (um para muitos)
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.User)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User) 
                .WithMany(u => u.Posts) 
                .HasForeignKey(p => p.UserId);

                modelBuilder.Entity<Post>()
                .HasMany(p => p.Shares)
                .WithOne(s => s.Post)
                .HasForeignKey(s => s.PostId);

                modelBuilder.Entity<Event>()
                .HasMany(e => e.Shares)
                .WithOne(s => s.Event)
                .HasForeignKey(s => s.EventId);

            // Relacionamento User-PostComment
            /*modelBuilder.Entity<User>()
                .HasMany(u => u.PostComments) 
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);*/

            // Relacionamento User-EventComment
            /*modelBuilder.Entity<User>()
                .HasMany(u => u.EventComments) 
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);*/

            // Relacionamento User-PostShare
            /*modelBuilder.Entity<User>()
                .HasMany(u => u.PostShares)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId);*/

            // Relacionamento User-EventShare
            /*modelBuilder.Entity<User>()
                .HasMany(u => u.EventShares)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId);*/

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments) // Corrigindo a propriedade para PostComment
                .WithOne(c => c.Post) 
                .HasForeignKey(c => c.PostId);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Comments)
                .WithOne(c => c.Event)
                .HasForeignKey(c => c.EventId);

            modelBuilder.Entity<PostReaction>()
                .Property(r => r.ReactionType)
                .HasConversion<int>();

            // Relacionamento Notification-User (um para muitos)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId);

             modelBuilder.Entity<Connection>()
                .HasKey(c => new { c.UserId, c.ConnectedUserId }); // Composite key

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.User)
                .WithMany(u => u.Connections)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Adjust DeleteBehavior as needed

            modelBuilder.Entity<Connection>()
                .HasOne(c => c.ConnectedUser)
                .WithMany(u => u.ConnectedTo) // Use ConnectedTo navigation property
                .HasForeignKey(c => c.ConnectedUserId)
                .OnDelete(DeleteBehavior.Restrict); // Adjust DeleteBehavior as needed

            // ... map other relationships ...

            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento PostComment-Post (um para muitos)
            modelBuilder.Entity<PostComment>()
                .HasOne(c => c.Post)
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento PostShare-Post (um para muitos)
            modelBuilder.Entity<PostShare>()
                .HasOne(s => s.Post)
                .WithMany(p => p.Shares)
                .HasForeignKey(s => s.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relacionamento EventComment-Event (um para muitos)
             modelBuilder.Entity<EventComment>()
                .HasOne(ec => ec.Event)
                .WithMany(e => e.Comments)
                .HasForeignKey(ec => ec.EventId)
                .OnDelete(DeleteBehavior.Cascade); // This might be OK; it's less of a problem if this isn't causing a cycle

            modelBuilder.Entity<EventComment>()
                .HasOne(ec => ec.User)
                .WithMany(u => u.EventComments)  // Ensure this navigation property is in your User entity
                .HasForeignKey(ec => ec.UserId)
                .OnDelete(DeleteBehavior.NoAction);

                modelBuilder.Entity<EventParticipant>()
            .HasOne(ep => ep.Event)
            .WithMany(e => e.Participants) // Assuming you have this navigation property in your Event class
            .HasForeignKey(ep => ep.EventId)
            .OnDelete(DeleteBehavior.Restrict); // Prevents deleting Events if EventParticipants exist

        modelBuilder.Entity<EventParticipant>()
            .HasOne(ep => ep.User)
            .WithMany(u => u.EventParticipants) // Assuming you have this navigation property in your User class
            .HasForeignKey(ep => ep.UserId)
            .OnDelete(DeleteBehavior.Restrict); 

            // Relacionamento EventShare-Event (um para muitos)
            modelBuilder.Entity<EventShare>()
                .HasOne(es => es.Event)
                .WithMany(e => e.Shares) // Make sure you have this navigation property in your Event class
                .HasForeignKey(es => es.EventId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents deleting Events if EventShares exist

            modelBuilder.Entity<EventShare>()
                .HasOne(es => es.User)
                .WithMany(u => u.EventShares) // Make sure you have this navigation property in your User class
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento PostComment-User (um para muitos)
            /*modelBuilder.Entity<PostComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.PostComments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);*/

            // Relacionamento EventComment-User (um para muitos)
            /*modelBuilder.Entity<EventComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.EventComments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);*/

            /*modelBuilder.Entity<PostShare>()
                .HasOne(s => s.User) // 's' para o PostShare
                .WithMany(u => u.PostShares)
                .HasForeignKey(s => s.UserId) // 's' para o PostShare
                .OnDelete(DeleteBehavior.Cascade);*/

            // Relacionamento EventShare-User (um para muitos)
            /*modelBuilder.Entity<EventShare>()
                .HasOne(s => s.User) // 's' para o EventShare
                .WithMany(u => u.EventShares)
                .HasForeignKey(s => s.UserId) // 's' para o EventShare
                .OnDelete(DeleteBehavior.Cascade);*/

            /*modelBuilder.Entity<PostComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.PostComments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);*/

            /*modelBuilder.Entity<EventComment>()
                .HasOne(c => c.User)
                .WithMany(u => u.EventComments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);*/

        }
    }
}