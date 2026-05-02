using ExpenseTracker.API.DTOs.Transactions;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Repositories;
using ExpenseTracker.API.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace ExpenseTracker.Tests.Services;

/// <summary>
/// Unit tests for TransactionService.
/// Uses Moq to mock the ITransactionRepository — no database needed for these tests.
/// This is TDD-style: tests define expected behavior before implementation.
/// </summary>
public class TransactionServiceTests
{
    // Mock creates a fake ITransactionRepository that we control
    private readonly Mock<ITransactionRepository> _mockRepo = new();
    private readonly TransactionService _service;

    public TransactionServiceTests()
    {
        // Inject the mock into the service
        _service = new TransactionService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetTransactionsAsync_ReturnsMappedDtos()
    {
        // Arrange — set up fake data the mock will return
        var category = new Category { Id = 1, Name = "Food & Dining", Color = "#EF4444", Icon = "utensils" };
        var transactions = new List<Transaction>
        {
            new()
            {
                Id = 1, UserId = 42, Date = new DateTime(2025, 3, 10),
                Merchant = "Starbucks", Description = "STARBUCKS #12345",
                Amount = 4.75m, TransactionType = "Debit",
                AccountName = "Checking", CategoryId = 1, Category = category
            }
        };

        // Tell the mock what to return when GetFilteredAsync is called
        _mockRepo.Setup(r => r.GetFilteredAsync(
            42, null, null, null, null, null, null, 1, 50, "date", "desc"))
            .ReturnsAsync((transactions, 1));

        // Act
        var result = await _service.GetTransactionsAsync(42, new TransactionFilterDto());

        // Assert
        result.TotalCount.Should().Be(1);
        result.Data.Should().HaveCount(1);

        var dto = result.Data[0];
        dto.Id.Should().Be(1);
        dto.Merchant.Should().Be("Starbucks");
        dto.Amount.Should().Be(4.75m);
        dto.CategoryName.Should().Be("Food & Dining");
        dto.CategoryColor.Should().Be("#EF4444");
    }

    [Fact]
    public async Task GetTransactionsAsync_ClampsPageSizeTo200()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetFilteredAsync(
            It.IsAny<int>(), null, null, null, null, null, null,
            1, 200, "date", "desc"))
            .ReturnsAsync((new List<Transaction>(), 0));

        // Act — request a page size above the 200 limit
        var filter = new TransactionFilterDto { PageSize = 99999 };
        await _service.GetTransactionsAsync(1, filter);

        // Assert — verify the clamped value was used
        _mockRepo.Verify(r => r.GetFilteredAsync(
            It.IsAny<int>(), null, null, null, null, null, null,
            1, 200, "date", "desc"), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTransactionNotFound()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetByIdAsync(999, 42))
            .ReturnsAsync((Transaction?)null);

        // Act
        var result = await _service.GetByIdAsync(999, 42);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        // Arrange
        _mockRepo.Setup(r => r.DeleteAsync(1, 42)).Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(1, 42);

        // Assert — verify the repo was actually called
        _mockRepo.Verify(r => r.DeleteAsync(1, 42), Times.Once);
    }
}
