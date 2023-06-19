using FC.CodeFlix.Catalog.Api.ApiModels.CastMember;
using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.Common;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.CreateCastMember;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.DeleteCastMember;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.GetCastMember;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.ListCastMembers;
using FC.CodeFlix.Catalog.Application.UseCases.CastMember.UpdateCastMember;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FC.CodeFlix.Catalog.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class CastMembersController : ControllerBase
{

    private readonly IMediator _mediator;

    public CastMembersController(IMediator mediator)
        => _mediator = mediator;

    [HttpPost()]
    [ProducesResponseType(typeof(ApiResponse<CastMemberModelOutput>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateCastMember([FromBody] CreateCastMemberInput input, CancellationToken cancellationToken)
    {

        var output = await _mediator.Send(input, cancellationToken);

        return CreatedAtAction(nameof(GetById),
                              new { id = output.Id },
                              new ApiResponse<CastMemberModelOutput>(output));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CastMemberModelOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateCastMember([FromRoute] Guid id, [FromBody] UpdateCastMemberApiInput apiInput , CancellationToken cancellationToken)
    {

        var input = new UpdateCastMemberInput(id, apiInput.Name, apiInput.Type);

        var output = await _mediator.Send(input, cancellationToken);

        return Ok(new ApiResponse<CastMemberModelOutput>(output));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CastMemberModelOutput>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id,
                                         CancellationToken cancellationToken)
    {
        var output = await _mediator.Send(new GetCastMemberInput(id), cancellationToken);

        return Ok(new ApiResponse<CastMemberModelOutput>(output));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType( StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteById([FromRoute] Guid id,
                                                CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCastMemberInput(id), cancellationToken);

        return NoContent();
    }


    [HttpGet()]
    [ProducesResponseType(typeof(ApiResponseList<CastMemberModelOutput>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken cancellationToken,
                                            [FromQuery] int? page = null,
                                            [FromQuery(Name = "per_page")] int? perPage = null,
                                            [FromQuery] string? search = null,
                                            [FromQuery] string? sort = null,
                                            [FromQuery] SearchOrder? dir = null
                                        )
    {
        var input = new ListCastMembersInput();

        if (page is not null) input.Page = page.Value;
        if (perPage is not null) input.PerPage = perPage.Value;
        if (!string.IsNullOrWhiteSpace(search)) input.Search = search;
        if (!string.IsNullOrWhiteSpace(sort)) input.Sort = sort;
        if (dir is not null) input.Dir = dir.Value;

        var output = await _mediator.Send(input, cancellationToken);

        return Ok(new ApiResponseList<CastMemberModelOutput>(output));
    }
}
