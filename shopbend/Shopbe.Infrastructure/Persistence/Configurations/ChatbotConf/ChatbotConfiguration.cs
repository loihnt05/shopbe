using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shopbe.Domain.Entities.Chatbot;

namespace Shopbe.Infrastructure.Persistence.Configurations.ChatbotConf;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasMaxLength(32)
            .HasDefaultValue("active");

        builder.Property(x => x.StartedAt)
            .HasDefaultValueSql("NOW()")
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Status });

        builder.HasMany(x => x.ChatMessages)
            .WithOne(x => x.Conversation)
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Sender)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        // BaseEntity likely provides CreatedAt/UpdatedAt, but we don't assume column names here.
        builder.HasIndex(x => x.ConversationId);
    }
}

