using ExpenseTracker.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Data;

/// <summary>
/// Seeds the database with default system categories on first run.
/// System categories are shared across all users and cannot be deleted.
/// This runs automatically on application startup via Program.cs.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Default expense categories with colors and icon identifiers.
    /// Colors are chosen to be visually distinct for chart display.
    /// Icons match the icon names used in the React frontend.
    /// </summary>
    private static readonly List<Category> DefaultCategories = new()
    {
        new() { Name = "Food & Dining",      Color = "#EF4444", Icon = "utensils",   IsSystem = true },
        new() { Name = "Transportation",     Color = "#3B82F6", Icon = "car",        IsSystem = true },
        new() { Name = "Shopping",           Color = "#8B5CF6", Icon = "shopping-bag", IsSystem = true },
        new() { Name = "Entertainment",      Color = "#F59E0B", Icon = "tv",         IsSystem = true },
        new() { Name = "Healthcare",         Color = "#10B981", Icon = "heart",      IsSystem = true },
        new() { Name = "Utilities",          Color = "#6366F1", Icon = "zap",        IsSystem = true },
        new() { Name = "Housing",            Color = "#EC4899", Icon = "home",       IsSystem = true },
        new() { Name = "Travel",             Color = "#14B8A6", Icon = "plane",      IsSystem = true },
        new() { Name = "Education",          Color = "#F97316", Icon = "book",       IsSystem = true },
        new() { Name = "Personal Care",      Color = "#84CC16", Icon = "scissors",   IsSystem = true },
        new() { Name = "Insurance",          Color = "#0EA5E9", Icon = "shield",     IsSystem = true },
        new() { Name = "Subscriptions",      Color = "#A855F7", Icon = "repeat",     IsSystem = true },
        new() { Name = "Income",             Color = "#22C55E", Icon = "trending-up", IsSystem = true },
        new() { Name = "Transfers",          Color = "#94A3B8", Icon = "arrows-right-left", IsSystem = true },
        new() { Name = "Uncategorized",      Color = "#6B7280", Icon = "tag",        IsSystem = true },
    };

    /// <summary>
    /// Called from Program.cs on startup.
    /// Only seeds categories that don't already exist (idempotent).
    /// </summary>
    public static async Task SeedAsync(AppDbContext context)
    {
        // Run any pending database migrations automatically
        await context.Database.MigrateAsync();

        // Only seed if no system categories exist yet
        if (await context.Categories.AnyAsync(c => c.IsSystem))
            return;

        await context.Categories.AddRangeAsync(DefaultCategories);
        await context.SaveChangesAsync();
    }
}
