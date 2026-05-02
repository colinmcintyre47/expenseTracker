using ExpenseTracker.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>Swashbuckle requires IFormFile to be in a wrapper class, not decorated with [FromForm] directly.</summary>
public class UploadStatementRequest
{
    public IFormFile File { get; set; } = null!;
    public string BankName { get; set; } = string.Empty;
    public int? StatementYear { get; set; }
}

/// <summary>
/// Handles bank statement uploads (CSV and PDF).
/// </summary>
[Route("api/statements")]
public class StatementsController : BaseAuthController
{
    private readonly IStatementService _statementService;

    public StatementsController(IStatementService statementService)
    {
        _statementService = statementService;
    }

    /// <summary>
    /// Upload a PNC bank statement (CSV or PDF).
    /// The file is parsed, categorized, and stored in the database.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10_485_760)] // 10MB max file size
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload([FromForm] UploadStatementRequest request)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        var ext = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (ext != ".csv" && ext != ".pdf")
            return BadRequest(new { message = "Only CSV and PDF files are supported." });

        if (string.IsNullOrEmpty(request.BankName))
            return BadRequest(new { message = "Bank name is required." });

        using var stream = request.File.OpenReadStream();
        var result = await _statementService.ProcessUploadAsync(
            stream, request.File.FileName, request.BankName, UserId, request.StatementYear);

        return Ok(result);
    }

    /// <summary>Returns a list of all statements uploaded by the current user.</summary>
    [HttpGet]
    public async Task<IActionResult> GetStatements()
    {
        var statements = await _statementService.GetStatementsAsync(UserId);
        return Ok(statements);
    }
}
