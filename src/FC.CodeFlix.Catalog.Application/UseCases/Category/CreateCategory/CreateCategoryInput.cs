﻿using FC.CodeFlix.Catalog.Application.UseCases.Category.Common;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Category.CreateCategory;

public class CreateCategoryInput : IRequest<CategoryModelOutput>
{
    public CreateCategoryInput(string name, string? description = null, bool isActive = true)
    {
        Name = name;
        Description = description ?? string.Empty;
        IsActive = isActive;
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public bool IsActive { get; set; }

}
