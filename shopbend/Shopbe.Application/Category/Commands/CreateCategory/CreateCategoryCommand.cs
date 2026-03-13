using MediatR;
using Shopbe.Application.Category.Dtos;

namespace Shopbe.Application.Category.Commands.CreateCategory;

public record CreateCategoryCommand(CategoryRequestDto Request) : IRequest<CategoryResponseDto>;