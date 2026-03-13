using MediatR;
using Shopbe.Application.Category.Dtos;

namespace Shopbe.Application.Category.Queries.GetAllCategories;

public record GetAllCategoriesQuery(CategoryQueryDto Filter) : IRequest<IEnumerable<CategoryResponseDto>>;
