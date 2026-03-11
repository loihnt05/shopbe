using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shopbe.Domain.Entities;

namespace Shopbe.Web.Controllers;

public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    public CategoryController(IMediator mediator)
    {
        _mediator = mediator;
    }
}