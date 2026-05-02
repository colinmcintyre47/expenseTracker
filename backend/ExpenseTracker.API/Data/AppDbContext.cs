using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Data;

/// <summary>
/// Entity Framework Core database context.
/// This is the single entry point for all database operations.
///
/// How EF Core works:
/// 1. DbSet properties represent database tables.
/// 2. OnModelCreating() configures table relationships, indexes, and constraints.
/// 3. EF Core generates SQL from LINQ queries at runtime.
/// 4. Migrations track schema changes and apply them to the database.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Each DbSet<T> maps to a database table
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<UploadedStatement> UploadedStatements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique(); // Email must be unique
            entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
            entity.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
        });

        // --- Category ---
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Color).HasMaxLength(7).HasDefaultValue("#6B7280");
            entity.Property(c => c.Icon).HasMaxLength(50).HasDefaultValue("tag");

            // A category can belong to a user (custom) or be null (system default)
            entity.HasOne(c => c.User)
                  .WithMany(u => u.Categories)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .IsRequired(false);
        });

        // --- Transaction ---
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.Property(t => t.Merchant).HasMaxLength(255);
            entity.Property(t => t.Description).HasColumnType("text");
            entity.Property(t => t.Amount).HasPrecision(10, 2);
            entity.Property(t => t.TransactionType).HasMaxLength(20);
            entity.Property(t => t.AccountName).HasMaxLength(100);

            // ImportHash must be unique to prevent duplicate imports
            entity.HasIndex(t => t.ImportHash).IsUnique();
            entity.Property(t => t.ImportHash).HasMaxLength(64).IsRequired();

            // Composite indexes for common query patterns
            entity.HasIndex(t => new { t.UserId, t.Date })
                  .HasDatabaseName("idx_user_date");
            entity.HasIndex(t => new { t.UserId, t.CategoryId })
                  .HasDatabaseName("idx_user_category");

            // A transaction belongs to a user
            entity.HasOne(t => t.User)
                  .WithMany(u => u.Transactions)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // A transaction belongs to a category
            entity.HasOne(t => t.Category)
                  .WithMany(c => c.Transactions)
                  .HasForeignKey(t => t.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict); // Don't delete categories that have transactions

            // A transaction optionally belongs to an uploaded statement
            entity.HasOne(t => t.Statement)
                  .WithMany(s => s.Transactions)
                  .HasForeignKey(t => t.StatementId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        // --- Budget ---
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.Property(b => b.MonthlyLimit).HasPrecision(10, 2);

            // Prevent duplicate budgets for same category/month/year
            entity.HasIndex(b => new { b.UserId, b.CategoryId, b.Month, b.Year })
                  .IsUnique()
                  .HasDatabaseName("unique_budget");

            entity.HasOne(b => b.User)
                  .WithMany(u => u.Budgets)
                  .HasForeignKey(b => b.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(b => b.Category)
                  .WithMany(c => c.Budgets)
                  .HasForeignKey(b => b.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Alert ---
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.Property(a => a.Type).HasMaxLength(50);
            entity.Property(a => a.Message).HasColumnType("text");

            // Index for quickly fetching unread alerts for a user
            entity.HasIndex(a => new { a.UserId, a.IsRead })
                  .HasDatabaseName("idx_user_unread");

            entity.HasOne(a => a.User)
                  .WithMany(u => u.Alerts)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Alert may reference a specific transaction
            entity.HasOne(a => a.Transaction)
                  .WithMany(t => t.Alerts)
                  .HasForeignKey(a => a.TransactionId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        // --- UploadedStatement ---
        modelBuilder.Entity<UploadedStatement>(entity =>
        {
            entity.Property(s => s.BankName).HasMaxLength(100);
            entity.Property(s => s.FileName).HasMaxLength(255);
            entity.Property(s => s.Status).HasMaxLength(50).HasDefaultValue("Processing");

            entity.HasOne(s => s.User)
                  .WithMany(u => u.UploadedStatements)
                  .HasForeignKey(s => s.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
