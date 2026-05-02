using ExpenseTracker.API.Repositories;

namespace ExpenseTracker.API.Services;

/// <summary>
/// Rule-based transaction categorization using keyword matching.
///
/// HOW IT WORKS:
/// 1. We maintain a dictionary mapping keywords → category names.
/// 2. For each transaction, we check if its description contains any keyword.
/// 3. First match wins (rules are checked in order).
/// 4. If no match, we fall back to "Uncategorized".
///
/// This approach is fast and transparent — a developer can easily see and adjust
/// why any transaction was categorized a certain way.
///
/// FUTURE ENHANCEMENT:
/// Replace this service with GeminiCategorizationService that sends the description
/// to Google Gemini API for intelligent categorization.
/// The interface contract (ICategorizationService) stays the same.
/// </summary>
public class RuleBasedCategorizationService : ICategorizationService
{
    private readonly ICategoryRepository _categoryRepo;

    // Keyword rules: maps merchant keywords (lowercase) → category name
    // Rules are checked in order — more specific rules should come first.
    private static readonly List<(string Keyword, string CategoryName)> Rules = new()
    {
        // Food & Dining
        ("starbucks",       "Food & Dining"),
        ("dunkin",          "Food & Dining"),
        ("mcdonald",        "Food & Dining"),
        ("chick-fil-a",     "Food & Dining"),
        ("chipotle",        "Food & Dining"),
        ("subway",          "Food & Dining"),
        ("panera",          "Food & Dining"),
        ("domino",          "Food & Dining"),
        ("pizza",           "Food & Dining"),
        ("restaurant",      "Food & Dining"),
        ("grubhub",         "Food & Dining"),
        ("doordash",        "Food & Dining"),
        ("ubereats",        "Food & Dining"),
        ("instacart",       "Food & Dining"),
        ("whole foods",     "Food & Dining"),
        ("trader joe",      "Food & Dining"),
        ("kroger",          "Food & Dining"),
        ("wegmans",         "Food & Dining"),
        ("giant eagle",     "Food & Dining"),
        ("safeway",         "Food & Dining"),

        // Transportation
        ("uber",            "Transportation"),
        ("lyft",            "Transportation"),
        ("gas station",     "Transportation"),
        ("shell",           "Transportation"),
        ("bp",              "Transportation"),
        ("exxon",           "Transportation"),
        ("chevron",         "Transportation"),
        ("sunoco",          "Transportation"),
        ("speedway",        "Transportation"),
        ("e-zpass",         "Transportation"),
        ("parking",         "Transportation"),
        ("transit",         "Transportation"),

        // Shopping
        ("amazon",          "Shopping"),
        ("walmart",         "Shopping"),
        ("target",          "Shopping"),
        ("best buy",        "Shopping"),
        ("costco",          "Shopping"),
        ("ebay",            "Shopping"),
        ("etsy",            "Shopping"),
        ("home depot",      "Shopping"),
        ("lowes",           "Shopping"),
        ("ikea",            "Shopping"),
        ("macy",            "Shopping"),
        ("nordstrom",       "Shopping"),

        // Entertainment
        ("netflix",         "Entertainment"),
        ("spotify",         "Entertainment"),
        ("hulu",            "Entertainment"),
        ("disney",          "Entertainment"),
        ("hbo",             "Entertainment"),
        ("apple",           "Entertainment"),   // Apple TV+, App Store, iTunes
        ("playstation",     "Entertainment"),
        ("xbox",            "Entertainment"),
        ("steam",           "Entertainment"),
        ("amc",             "Entertainment"),
        ("regal",           "Entertainment"),
        ("movie",           "Entertainment"),
        ("theater",         "Entertainment"),
        ("concert",         "Entertainment"),
        ("ticketmaster",    "Entertainment"),
        ("twitch",          "Entertainment"),

        // Healthcare
        ("cvs",             "Healthcare"),
        ("walgreen",        "Healthcare"),
        ("rite aid",        "Healthcare"),
        ("pharmacy",        "Healthcare"),
        ("clinic",          "Healthcare"),
        ("hospital",        "Healthcare"),
        ("doctor",          "Healthcare"),
        ("dental",          "Healthcare"),
        ("vision",          "Healthcare"),
        ("medical",         "Healthcare"),

        // Utilities
        ("electric",        "Utilities"),
        ("gas company",     "Utilities"),
        ("water bill",      "Utilities"),
        ("internet",        "Utilities"),
        ("comcast",         "Utilities"),
        ("verizon",         "Utilities"),
        ("at&t",            "Utilities"),
        ("t-mobile",        "Utilities"),
        ("spectrum",        "Utilities"),

        // Housing
        ("rent",            "Housing"),
        ("mortgage",        "Housing"),
        ("hoa",             "Housing"),
        ("property tax",    "Housing"),

        // Travel
        ("airbnb",          "Travel"),
        ("hotel",           "Travel"),
        ("marriott",        "Travel"),
        ("hilton",          "Travel"),
        ("expedia",         "Travel"),
        ("booking.com",     "Travel"),
        ("delta",           "Travel"),
        ("united",          "Travel"),
        ("southwest",       "Travel"),
        ("american air",    "Travel"),

        // Education
        ("tuition",         "Education"),
        ("university",      "Education"),
        ("college",         "Education"),
        ("student loan",    "Education"),
        ("udemy",           "Education"),
        ("coursera",        "Education"),

        // Personal Care
        ("salon",           "Personal Care"),
        ("barber",          "Personal Care"),
        ("spa",             "Personal Care"),
        ("nail",            "Personal Care"),
        ("gym",             "Personal Care"),
        ("planet fitness",  "Personal Care"),

        // Insurance
        ("insurance",       "Insurance"),
        ("geico",           "Insurance"),
        ("progressive",     "Insurance"),
        ("allstate",        "Insurance"),

        // Subscriptions
        ("subscription",    "Subscriptions"),
        ("monthly",         "Subscriptions"),
        ("adobe",           "Subscriptions"),
        ("microsoft 365",   "Subscriptions"),

        // Income (Credits)
        ("payroll",         "Income"),
        ("direct deposit",  "Income"),
        ("salary",          "Income"),
        ("employer",        "Income"),
        ("zelle",           "Transfers"),
        ("venmo",           "Transfers"),
        ("paypal",          "Transfers"),
        ("transfer",        "Transfers"),
        ("wire",            "Transfers"),
    };

    public RuleBasedCategorizationService(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    public async Task<int> CategorizeAsync(string description, int uncategorizedCategoryId)
    {
        if (string.IsNullOrWhiteSpace(description))
            return uncategorizedCategoryId;

        var descLower = description.ToLowerInvariant();

        // Check each rule in order — return the first match
        foreach (var (keyword, categoryName) in Rules)
        {
            if (descLower.Contains(keyword))
            {
                // Look up the category by name in the database
                var categories = await _categoryRepo.GetForUserAsync(0); // 0 = system categories only
                var match = categories.FirstOrDefault(c =>
                    c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                    return match.Id;
            }
        }

        // No rule matched — return the "Uncategorized" category
        return uncategorizedCategoryId;
    }
}
