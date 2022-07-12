using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.Category.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.DeleteGenre;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.GetGenre;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FC.CodeFlix.Catalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GenresController(IMediator mediator)
            => _mediator = mediator;


        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryModelOutput>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id,
                                                 CancellationToken cancellationToken)
        {
            var output = await _mediator.Send(new GetGenreInput(id), cancellationToken);

            return Ok(new ApiResponse<GenreModelOutput>(output));
        }


        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryModelOutput>), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteById([FromRoute] Guid id,
                                                 CancellationToken cancellationToken)
        {
            var output = await _mediator.Send(new DeleteGenreInput(id), cancellationToken);

            return NoContent();
        }

    }
}