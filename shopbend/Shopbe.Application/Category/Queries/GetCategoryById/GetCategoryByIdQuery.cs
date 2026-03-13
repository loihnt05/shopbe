using MediatR;
using Shopbe.Application.Category.Dtos;

namespace Shopbe.Application.Category.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryResponseDto?>;
