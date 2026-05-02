using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTOs.Categories;

/// <summary>Category data returned to the frontend.</summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
}

/// <summary>Request body for POST /api/categories.</summary>
public class CreateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(7)]
    public string Color { get; set; } = "#6B7280";

    [MaxLength(50)]
    public string Icon { get; set; } = "tag";
}

/// <summary>Request body for PUT /api/categories/{id}.</summary>
public class UpdateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(7)]
    public string Color { get; set; } = "#6B7280";

    [MaxLength(50)]
    public string Icon { get; set; } = "tag";
}
