﻿using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Enum;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;


public record CreateVideoInput
    (string Title, 
    string Description, 
    int YearLaunched,
    bool Published,
    bool Opened,
    int Duration,
    Rating Rating,
    IReadOnlyCollection<Guid>? CategoriesId = null,
    IReadOnlyCollection<Guid>? GenresId = null,
    IReadOnlyCollection<Guid>? CastMembersId = null,
    FileInput? Thumb = null,
    FileInput? Banner = null,
    FileInput? ThumbHalf = null,
    FileInput? Media = null,
    FileInput? Trailer = null)
    :  IRequest<VideoModelOutput>;


