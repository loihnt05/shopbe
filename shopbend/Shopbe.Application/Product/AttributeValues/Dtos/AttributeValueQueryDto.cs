namespace Shopbe.Application.Product.AttributeValues.Dtos;

public record AttributeValueQueryDto(
    string? Value, 
    Guid? AttributeId, 
    int PageNumber = 1, 
    int PageSize = 10
    );