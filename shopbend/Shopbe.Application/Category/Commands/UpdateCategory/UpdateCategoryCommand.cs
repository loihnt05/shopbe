using MediatR;
using Shopbe.Application.Category.Dtos;

namespace Shopbe.Application.Category.Commands.UpdateCategory;

public record UpdateCategoryCommand(Guid Id, CategoryRequestDto Request) : IRequest<CategoryResponseDto>;
