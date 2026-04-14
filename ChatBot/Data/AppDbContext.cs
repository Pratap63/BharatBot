using Microsoft.EntityFrameworkCore;
using ChatBot.Models;   // ✅ REQUIRED

namespace ChatBot.Data;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatKnowledge>()
            .ToTable("ChatKnowledge")
            .HasKey(x => x.Id);

        modelBuilder.Entity<Chat>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }


    public DbSet<Chat> Chats { get; set; }
    public DbSet<Message> Message { get; set; }
    public DbSet<ChatKnowledge> ChatKnowledge { get; set; } = null!;
    public DbSet<User> Users { get; set; }
    public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }


}
