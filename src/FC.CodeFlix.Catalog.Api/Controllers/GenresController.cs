using FC.CodeFlix.Catalog.Api.ApiModels.Genre;
using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Application.UseCases.Category.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.CreateGenre;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.DeleteGenre;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.GetGenre;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.ListGenres;
using FC.CodeFlix.Catalog.Application.UseCases.Genre.UpdateGenre;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
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

        [HttpPost()]
        [ProducesResponseType(typeof(ApiResponse<GenreModelOutput>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateGenre([FromBody] CreateGenreInput input, CancellationToken cancellationToken)
        {

            var output = await _mediator.Send(input, cancellationToken);


            return CreatedAtAction(nameof(GetById),
                                  new { id = output.Id },
                                  new ApiResponse<GenreModelOutput>(output));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<GenreModelOutput>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> UpdateGenre([FromBody] UpdateGenreApiInput apiInput, [FromRoute] Guid id,  CancellationToken cancellationToken)
        {

            var output = await _mediator.Send(
                                             new UpdateGenreInput(id, 
                                                                  apiInput.Name,
                                                                  apiInput.IsActive,
                                                                  apiInput.CategoriesIds),
                                             cancellationToken);

            return Ok(new ApiResponse<GenreModelOutput>(output));
        }


        [HttpGet()]
        [ProducesResponseType(typeof(ApiResponseList<GenreModelOutput>), StatusCodes.Status200OK)]

        public async Task<IActionResult> List(CancellationToken cancellationToken,
                                            [FromQuery] int? page = null,
                                            [FromQuery(Name = "per_page")] int? perPage = null,
                                            [FromQuery] string? search = null,
                                            [FromQuery] string? sort = null,
                                            [FromQuery] SearchOrder? dir = null
                                        )
        {
            var input = new ListGenresInput();

            if (page is not null) input.Page = page.Value;
            if (perPage is not null) input.PerPage = perPage.Value;
            if (!string.IsNullOrWhiteSpace(search)) input.Search = search;
            if (!string.IsNullOrWhiteSpace(sort)) input.Sort = sort;
            if (dir is not null) input.Dir = dir.Value;

            var output = await _mediator.Send(input, cancellationToken);

            return Ok(new ApiResponseList<GenreModelOutput>(output));
        }
    }
}