﻿using FC.CodeFlix.Catalog.Api.ApiModels.Response;
using FC.CodeFlix.Catalog.Api.ApiModels.Video;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Video.GetVideo;
using FC.CodeFlix.Catalog.Application.UseCases.Video.ListVideos;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FC.CodeFlix.Catalog.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VideosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VideosController(IMediator mediator)
            => _mediator = mediator;

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VideoModelOutput>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateVideo([FromBody] CreateVideoApiInput request, CancellationToken cancellationToken)
        {
            var input = request.ToCreateVideoInput();

            var output = await _mediator.Send(input, cancellationToken);

            return CreatedAtAction(nameof(CreateVideo), new { id = output.Id }, new ApiResponse<VideoModelOutput>(output));
        }


        [HttpGet()]
        [ProducesResponseType(typeof(ApiResponseList<VideoModelOutput>), StatusCodes.Status200OK)]

        public async Task<IActionResult> List(CancellationToken cancellationToken,
                                           [FromQuery] int? page = null,
                                           [FromQuery(Name = "per_page")] int? perPage = null,
                                           [FromQuery] string? search = null,
                                           [FromQuery] string? sort = null,
                                           [FromQuery] SearchOrder? dir = null
                                       )
        {
            var input = new ListVideosInput();

            if (page is not null) input.Page = page.Value;
            if (perPage is not null) input.PerPage = perPage.Value;
            if (!string.IsNullOrWhiteSpace(search)) input.Search = search;
            if (!string.IsNullOrWhiteSpace(sort)) input.Sort = sort;
            if (dir is not null) input.Dir = dir.Value;

            var output = await _mediator.Send(input, cancellationToken);

            return Ok(new ApiResponseList<VideoModelOutput>(output));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<VideoModelOutput>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById([FromRoute] Guid id,
                                         CancellationToken cancellationToken)
        {
            var output = await _mediator.Send(new GetVideoInput(id), cancellationToken);

            return Ok(new ApiResponse<VideoModelOutput>(output));
        }
    }
}
