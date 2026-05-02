using ExpenseTracker.API.DTOs.Categories;
using ExpenseTracker.API.Models;
using ExpenseTracker.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Controllers;

/// <summary>
/// Category management endpoints.
/// System categories are read-only; user-created categories are fully editable.
/// </summary>
[Route("api/categories")]
public class CategoriesController : BaseAuthController
{
    private readonly ICategoryRepository _categoryRepo;

    public CategoriesController(ICategoryRepository categoryRepo)
    {
        _categoryRepo = categoryRepo;
    }

    /// <summary>Returns all categories available to the current user (system + their own).</summary>
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _categoryRepo.GetForUserAsync(UserId);
        var dtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Color = c.Color,
            Icon = c.Icon,
            IsSystem = c.IsSystem
        }).ToList();
        return Ok(dtos);
    }

    /// <summary>Create a custom category for the current user.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = new Category
        {
            UserId = UserId,
            Name = request.Name.Trim(),
            Color = request.Color,
            Icon = request.Icon,
            IsSystem = false
        };

        var created = await _categoryRepo.CreateAsync(category);
        return StatusCode(201, new CategoryDto
        {
            Id = created.Id,
            Name = created.Name,
            Color = created.Color,
            Icon = created.Icon,
            IsSystem = false
        });
    }

    /// <summary>Update a user-owned category (system categories cannot be modified).</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
    {
        var category = await _categoryRepo.GetByIdAsync(id);

        if (category == null)
            return NotFound();

        // Prevent modification of system categories or categories owned by other users
        if (category.IsSystem || category.UserId != UserId)
            return Forbid();

        category.Name = request.Name.Trim();
        category.Color = request.Color;
        category.Icon = request.Icon;

        await _categoryRepo.UpdateAsync(category);
        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Color = category.Color,
            Icon = category.Icon,
            IsSystem = false
        });
    }

    /// <summary>Delete a user-owned category.</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id);

        if (category == null) return NotFound();
        if (category.IsSystem || category.UserId != UserId) return Forbid();

        await _categoryRepo.DeleteAsync(id, UserId);
        return NoContent();
    }
}
