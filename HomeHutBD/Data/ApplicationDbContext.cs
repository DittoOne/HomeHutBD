using Microsoft.EntityFrameworkCore;
using HomeHutBD.Models;

namespace HomeHutBD.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Properties> Properties { get; set; }
        public DbSet<Chats> Chats { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<VerificationRequests> VerificationRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraints
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Username)
                .IsUnique();
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique();
            modelBuilder.Entity<Users>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Admin>()
                .HasIndex(a => a.Email)
                .IsUnique();

            modelBuilder.Entity<VerificationRequests>()
                .HasIndex(v => v.NidNumber)
                .IsUnique();

            // Relationships and delete behaviors

            // Users -> Properties (cascade delete)
            modelBuilder.Entity<Properties>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Properties -> VerificationRequests:
            // Use NoAction to avoid multiple cascade paths.
            modelBuilder.Entity<Properties>()
                .HasOne(p => p.VerificationRequest)
                .WithOne()
                .HasForeignKey<Properties>(p => p.VerificationId)
                .OnDelete(DeleteBehavior.NoAction);

            // Chats relationships:
            // Set both Sender and Receiver to NoAction to eliminate multiple cascade paths.
            modelBuilder.Entity<Chats>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Chats>()
                .HasOne(c => c.Receiver)
                .WithMany()
                .HasForeignKey(c => c.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction);

            // Chats -> Properties (cascade delete)
            modelBuilder.Entity<Chats>()
                .HasOne(c => c.Property)
                .WithMany()
                .HasForeignKey(c => c.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);

            // VerificationRequests relationships
            modelBuilder.Entity<VerificationRequests>()
                .HasOne(v => v.User)
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VerificationRequests>()
                .HasOne(v => v.Admin)
                .WithMany()
                .HasForeignKey(v => v.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
