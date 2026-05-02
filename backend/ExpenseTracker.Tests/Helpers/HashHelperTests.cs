using ExpenseTracker.API.Helpers;
using FluentAssertions;
using Xunit;

namespace ExpenseTracker.Tests.Helpers;

/// <summary>Tests for the transaction deduplication hash logic.</summary>
public class HashHelperTests
{
    [Fact]
    public void GenerateTransactionHash_SameInputs_ReturnsSameHash()
    {
        var hash1 = HashHelper.GenerateTransactionHash(1, new DateTime(2025, 3, 10), 4.75m, "STARBUCKS");
        var hash2 = HashHelper.GenerateTransactionHash(1, new DateTime(2025, 3, 10), 4.75m, "STARBUCKS");
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GenerateTransactionHash_DifferentUser_ReturnsDifferentHash()
    {
        var hash1 = HashHelper.GenerateTransactionHash(1, new DateTime(2025, 3, 10), 4.75m, "STARBUCKS");
        var hash2 = HashHelper.GenerateTransactionHash(2, new DateTime(2025, 3, 10), 4.75m, "STARBUCKS");
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void GenerateTransactionHash_DescriptionWhitespace_IsNormalized()
    {
        // Leading/trailing whitespace should not produce different hashes
        var hash1 = HashHelper.GenerateTransactionHash(1, new DateTime(2025, 3, 10), 4.75m, "STARBUCKS");
        var hash2 = HashHelper.GenerateTransactionHash(1, new DateTime(2025, 3, 10), 4.75m, "  STARBUCKS  ");
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GenerateTransactionHash_Returns64CharHexString()
    {
        var hash = HashHelper.GenerateTransactionHash(1, DateTime.Now, 10.00m, "TEST");
        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
    }
}
