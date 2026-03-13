using MediatR;

namespace Shopbe.Application.Category.Commands.DeleteCategory;

public record DeleteCategoryCommand(Guid Id) : IRequest<bool>;
