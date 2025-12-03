using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace service_csharp.Models;

[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("email")]
    public required string Email { get; set; }

    [Column("username")]
    public string? Username { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("avatar_url")]
    public string? AvatarUrl { get; set; }

    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("last_seen_at")]
    public DateTimeOffset? LastSeenAt { get; set; }
}

[Table("organizations")]
public class Organization
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("slug")]
    public required string Slug { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("projects")]
public class Project
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("org_id")]
    public Guid OrgId { get; set; }
    
    [ForeignKey("OrgId")]
    public Organization? Organization { get; set; }

    [Column("name")]
    public required string Name { get; set; }

    [Column("slug")]
    public string? Slug { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("boards")]
public class Board
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }

    [Column("title")]
    public required string Title { get; set; }

    [Column("settings", TypeName = "jsonb")]
    public JsonDocument? Settings { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("issues")]
public class Issue
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid ProjectId { get; set; }
    
    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }

    [Column("board_id")]
    public Guid? BoardId { get; set; }
    
    [ForeignKey("BoardId")]
    public Board? Board { get; set; }

    [Column("number")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Number { get; set; }

    [Column("title")]
    public required string Title { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("body", TypeName = "jsonb")]
    public JsonDocument? Body { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("priority")]
    public int? Priority { get; set; }

    [Column("assignee_id")]
    public Guid? AssigneeId { get; set; }
    
    [ForeignKey("AssigneeId")]
    public User? Assignee { get; set; }

    [Column("due_date")]
    public DateTimeOffset? DueDate { get; set; }

    [Column("labels")]
    public string[]? Labels { get; set; }

    [Column("custom_fields", TypeName = "jsonb")]
    public JsonDocument? CustomFields { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    public User? Creator { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Column("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("comments")]
public class Comment
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("issue_id")]
    public Guid IssueId { get; set; }
    
    [ForeignKey("IssueId")]
    public Issue? Issue { get; set; }

    [Column("author_id")]
    public Guid? AuthorId { get; set; }
    
    [ForeignKey("AuthorId")]
    public User? Author { get; set; }

    [Column("content", TypeName = "jsonb")]
    public JsonDocument Content { get; set; } = null!;

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("ai_conversations")]
public class AiConversation
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("project_id")]
    public Guid? ProjectId { get; set; }

    [Column("created_by")]
    public Guid? CreatedBy { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("metadata", TypeName = "jsonb")]
    public JsonDocument? Metadata { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

[Table("ai_messages")]
public class AiMessage
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("conversation_id")]
    public Guid ConversationId { get; set; }
    
    [ForeignKey("ConversationId")]
    public AiConversation? Conversation { get; set; }

    [Column("role")]
    public string? Role { get; set; } // 'user','assistant','system'

    [Column("content", TypeName = "jsonb")]
    public JsonDocument Content { get; set; } = null!;

    [Column("attachments", TypeName = "jsonb")]
    public JsonDocument? Attachments { get; set; }

    [Column("token_usage", TypeName = "jsonb")]
    public JsonDocument? TokenUsage { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
