using System.Text;
using ExpenseTracker.API.Parsers;
using FluentAssertions;
using Xunit;

namespace ExpenseTracker.Tests.Parsers;

/// <summary>
/// Unit tests for the PNC Bank CSV parser.
/// Tests cover: normal rows, credits, debits, duplicate-skipping, malformed rows.
///
/// HOW TO RUN:
///   cd backend
///   dotnet test
/// </summary>
public class PncBankParserTests
{
    private readonly PncBankParser _parser = new();

    /// <summary>Helper: converts a CSV string to a Stream for testing.</summary>
    private static Stream ToCsvStream(string csv)
        => new MemoryStream(Encoding.UTF8.GetBytes(csv));

    [Fact]
    public async Task ParseAsync_StandardFormat_ParsesDebitsCorrectly()
    {
        // Arrange
        var csv = """
            Date,Description,Withdrawals,Deposits,Balance
            01/15/2025,"STARBUCKS #12345 PITTSBURGH PA",-4.75,,1234.56
            01/14/2025,"AMAZON.COM*AB123456",-52.99,,1239.31
            """;

        // Act
        var result = await _parser.ParseAsync(ToCsvStream(csv));

        // Assert
        result.Rows.Should().HaveCount(2);
        result.Errors.Should().BeEmpty();

        var starbucks = result.Rows[0];
        starbucks.Date.Should().Be(new DateTime(2025, 1, 15));
        starbucks.Description.Should().Be("STARBUCKS #12345 PITTSBURGH PA");
        starbucks.Amount.Should().Be(4.75m);
        starbucks.TransactionType.Should().Be("Debit");
    }

    [Fact]
    public async Task ParseAsync_CreditRow_ParsedAsCredit()
    {
        // Arrange
        var csv = """
            Date,Description,Withdrawals,Deposits,Balance
            01/13/2025,"DIRECT DEPOSIT EMPLOYER",,2500.00,1292.30
            """;

        // Act
        var result = await _parser.ParseAsync(ToCsvStream(csv));

        // Assert
        result.Rows.Should().HaveCount(1);
        var row = result.Rows[0];
        row.Amount.Should().Be(2500.00m);
        row.TransactionType.Should().Be("Credit");
    }

    [Fact]
    public async Task ParseAsync_EmptyRows_AreSkipped()
    {
        // Arrange — row with no withdrawal or deposit amount should be skipped
        var csv = """
            Date,Description,Withdrawals,Deposits,Balance
            01/15/2025,"STARBUCKS",-4.75,,1234.56
            01/14/2025,"EMPTY ROW",,,
            """;

        // Act
        var result = await _parser.ParseAsync(ToCsvStream(csv));

        // Assert
        result.Rows.Should().HaveCount(1);
    }

    [Fact]
    public async Task ParseAsync_WithMetadataHeader_SkipsToDataRow()
    {
        // Some PNC exports include account info before the CSV headers
        var csv = """
            Account Number: ****1234
            Account Type: Virtual Wallet
            Export Date: 01/15/2025

            Date,Description,Withdrawals,Deposits,Balance
            01/15/2025,"TARGET #1234",-35.50,,500.00
            """;

        // Act
        var result = await _parser.ParseAsync(ToCsvStream(csv));

        // Assert
        result.Rows.Should().HaveCount(1);
        result.Rows[0].Merchant.Should().BeEmpty(); // Merchant set by service, not parser
    }

    [Fact]
    public async Task ParseAsync_EmptyCsv_ReturnsError()
    {
        // Act
        var result = await _parser.ParseAsync(ToCsvStream(string.Empty));

        // Assert
        result.Rows.Should().BeEmpty();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ParseAsync_InvalidDateFormat_AddsError()
    {
        // Arrange
        var csv = """
            Date,Description,Withdrawals,Deposits,Balance
            BAD-DATE,"STARBUCKS",-4.75,,1234.56
            """;

        // Act
        var result = await _parser.ParseAsync(ToCsvStream(csv));

        // Assert — bad row should be reported as an error, not crash the parser
        result.Errors.Should().NotBeEmpty();
        result.Rows.Should().BeEmpty();
    }
}
