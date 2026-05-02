namespace ExpenseTracker.API.Services;

/// <summary>
/// Interface for transaction categorization.
///
/// TODAY: RuleBasedCategorizationService — fast, deterministic, no API calls.
/// FUTURE: GeminiCategorizationService — calls Google Gemini API for AI-powered categorization.
///
/// To switch: change the DI registration in Program.cs from:
///   services.AddScoped&lt;ICategorizationService, RuleBasedCategorizationService&gt;();
/// to:
///   services.AddScoped&lt;ICategorizationService, GeminiCategorizationService&gt;();
///
/// The rest of the system (StatementService, etc.) doesn't change at all.
/// </summary>
public interface ICategorizationService
{
    /// <summary>
    /// Assigns a category ID to a transaction based on its description.
    /// Returns the ID of the matching category, or the "Uncategorized" category ID if no match.
    /// </summary>
    Task<int> CategorizeAsync(string description, int uncategorizedCategoryId);
}
