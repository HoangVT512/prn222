using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace InternManagement.Models;

public partial class InternmanagementContext : DbContext
{
    public InternmanagementContext()
    {
    }

    public InternmanagementContext(DbContextOptions<InternmanagementContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Conversation> Conversations { get; set; }
    public virtual DbSet<ConversationMember> ConversationMembers { get; set; }

    public virtual DbSet<Dailytracking> Dailytrackings { get; set; }

    public virtual DbSet<Efmigrationshistory> Efmigrationshistories { get; set; }

    public virtual DbSet<Evaluation> Evaluations { get; set; }

    public virtual DbSet<Interngroup> Interngroups { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Taskstatus> Taskstatuses { get; set; }

    public virtual DbSet<Tasksubmit> Tasksubmits { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=internmanagement;uid=root;pwd=1234", Microsoft.EntityFrameworkCore.ServerVersion.Parse("5.7.29-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("latin1_swedish_ci")
            .HasCharSet("latin1");

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("conversation");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasMany(d => d.Users).WithMany(p => p.Conversations)
                .UsingEntity<Dictionary<string, object>>(
                    "Conversationmember",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("conversationmember_ibfk_2"),
                    l => l.HasOne<Conversation>().WithMany()
                        .HasForeignKey("ConversationId")
                        .HasConstraintName("conversationmember_ibfk_1"),
                    j =>
                    {
                        j.HasKey("ConversationId", "UserId")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("conversationmember");
                        j.HasIndex(new[] { "UserId" }, "UserId");
                        j.IndexerProperty<int>("ConversationId").HasColumnType("int(11)");
                        j.IndexerProperty<int>("UserId").HasColumnType("int(11)");
                    });
        });

        modelBuilder.Entity<ConversationMember>()
               .HasKey(cm => new { cm.ConversationId, cm.UserId });

        modelBuilder.Entity<ConversationMember>()
            .ToTable("ConversationMember"); 


        modelBuilder.Entity<Dailytracking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("dailytracking");

            entity.HasIndex(e => e.StudentId, "StudentId");

            entity.HasIndex(e => e.TaskId, "TaskId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Report).HasColumnType("text");
            entity.Property(e => e.StudentId).HasColumnType("int(11)");
            entity.Property(e => e.TaskId).HasColumnType("int(11)");

            entity.HasOne(d => d.Student).WithMany(p => p.Dailytrackings)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("dailytracking_ibfk_1");

            entity.HasOne(d => d.Task).WithMany(p => p.Dailytrackings)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("dailytracking_ibfk_2");
        });

        modelBuilder.Entity<Efmigrationshistory>(entity =>
        {
            entity.HasKey(e => e.MigrationId).HasName("PRIMARY");

            entity
                .ToTable("__efmigrationshistory")
                .HasCharSet("utf8mb4")
                .UseCollation("utf8mb4_general_ci");

            entity.Property(e => e.MigrationId).HasMaxLength(150);
            entity.Property(e => e.ProductVersion).HasMaxLength(32);
        });

        modelBuilder.Entity<Evaluation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("evaluation");

            entity.HasIndex(e => e.MentorId, "idx_evaluation_mentor");

            entity.HasIndex(e => e.StudentId, "idx_evaluation_student");

            entity.HasIndex(e => e.TaskId, "idx_evaluation_task");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Comment).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.MentorId).HasColumnType("int(11)");
            entity.Property(e => e.Rating).HasColumnType("int(11)");
            entity.Property(e => e.StudentId).HasColumnType("int(11)");
            entity.Property(e => e.TaskId).HasColumnType("int(11)");

            entity.HasOne(d => d.Mentor).WithMany(p => p.EvaluationMentors)
                .HasForeignKey(d => d.MentorId)
                .HasConstraintName("evaluation_ibfk_2");

            entity.HasOne(d => d.Student).WithMany(p => p.EvaluationStudents)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("evaluation_ibfk_3");

            entity.HasOne(d => d.Task).WithMany(p => p.Evaluations)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("evaluation_ibfk_1");
        });

        modelBuilder.Entity<Interngroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("interngroup");

            entity.HasIndex(e => e.MentorId, "fk_internGroup_mentor");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Field)
                .HasMaxLength(255)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.MentorId).HasColumnType("int(11)");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");

            entity.HasOne(d => d.Mentor).WithMany(p => p.Interngroups)
                .HasForeignKey(d => d.MentorId)
                .HasConstraintName("fk_internGroup_mentor");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("message");

            entity.HasIndex(e => e.ConversationId, "ConversationId");

            entity.HasIndex(e => e.SenderId, "SenderId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.ConversationId).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.SenderId).HasColumnType("int(11)");

            entity.HasOne(d => d.Conversation).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ConversationId)
                .HasConstraintName("message_ibfk_1");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("message_ibfk_2");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("notification");

            entity.HasIndex(e => e.UserId, "idx_notification_user");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValueSql("'0'");
            entity.Property(e => e.StatusId).HasColumnType("int(11)");
            entity.Property(e => e.UserId).HasColumnType("int(11)");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("notification_ibfk_1");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("role");

            entity.HasIndex(e => e.Name, "Name").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("task");

            entity.HasIndex(e => e.GroupId, "idx_task_group");

            entity.HasIndex(e => e.MentorId, "idx_task_mentor");

            entity.HasIndex(e => e.StatusId, "idx_task_status");

            entity.HasIndex(e => e.StudentId, "idx_task_student");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("text");
            entity.Property(e => e.File).HasMaxLength(500);
            entity.Property(e => e.GroupId).HasColumnType("int(11)");
            entity.Property(e => e.MentorId).HasColumnType("int(11)");
            entity.Property(e => e.StatusId).HasColumnType("int(11)");
            entity.Property(e => e.StudentId).HasColumnType("int(11)");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Group).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.GroupId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("task_ibfk_3");

            entity.HasOne(d => d.Mentor).WithMany(p => p.TaskMentors)
                .HasForeignKey(d => d.MentorId)
                .HasConstraintName("task_ibfk_2");

            entity.HasOne(d => d.Status).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_ibfk_1");

            entity.HasOne(d => d.Student).WithMany(p => p.TaskStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("task_ibfk_4");
        });

        modelBuilder.Entity<Taskstatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("taskstatus");

            entity.HasIndex(e => e.Name, "Name").IsUnique();

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Tasksubmit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tasksubmit");

            entity.HasIndex(e => e.StudentId, "idx_task_submit_student");

            entity.HasIndex(e => e.TaskId, "idx_task_submit_task");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.MentorNote).HasColumnType("text");
            entity.Property(e => e.ProgressEvaluate).HasColumnType("int(11)");
            entity.Property(e => e.Remarks).HasColumnType("text");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.StudentId).HasColumnType("int(11)");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.SubmittedFile).HasMaxLength(500);
            entity.Property(e => e.TaskId).HasColumnType("int(11)");

            entity.HasOne(d => d.Student).WithMany(p => p.Tasksubmits)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("tasksubmit_ibfk_2");

            entity.HasOne(d => d.Task).WithMany(p => p.Tasksubmits)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("tasksubmit_ibfk_1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "Email").IsUnique();

            entity.HasIndex(e => e.GroupId, "GroupId");

            entity.HasIndex(e => e.ProviderId, "ProviderId").IsUnique();

            entity.HasIndex(e => e.Username, "Username").IsUnique();

            entity.HasIndex(e => e.RoleId, "idx_user_role");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .UseCollation("utf8_general_ci")
                .HasCharSet("utf8");
            entity.Property(e => e.GroupId).HasColumnType("int(11)");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("'1'");
            entity.Property(e => e.LoginProvider)
                .HasDefaultValueSql("'LOCAL'")
                .HasColumnType("enum('LOCAL','GOOGLE')");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Pfp).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RoleId).HasColumnType("int(11)");
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasOne(d => d.Group).WithMany(p => p.Users)
                .HasForeignKey(d => d.GroupId)
                .HasConstraintName("user_ibfk_2");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("user_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
