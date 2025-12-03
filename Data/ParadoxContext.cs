using Microsoft.EntityFrameworkCore;
using service_csharp.Models;

namespace service_csharp.Data;

public class ParadoxContext : DbContext
{
    public ParadoxContext(DbContextOptions<ParadoxContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Board> Boards { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<AiConversation> AiConversations { get; set; }
    public DbSet<AiMessage> AiMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración adicional si es necesaria
        modelBuilder.Entity<Issue>()
            .Property(i => i.Number)
            .ValueGeneratedOnAdd();
            
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("uuid-ossp");
    }
}
