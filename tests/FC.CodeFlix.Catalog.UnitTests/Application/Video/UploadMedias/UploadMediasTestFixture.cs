using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using UseCase = FC.CodeFlix.Catalog.Application.UseCases.Video.UploadMedias;
using System;
using Xunit;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using Moq;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.UploadMedias;

[CollectionDefinition(nameof(UploadMediasTestFixture))]
public class UploadMediasTestFixtureCollection : ICollectionFixture<UploadMediasTestFixture>
{
}

public class UploadMediasTestFixture : VideoBaseFixture
{
    public UseCase.UploadMediasInput GetValidInput(
        Guid? videoId = null,
        bool withVideoFile = true,
        bool withTrailerFile = true
        )
        => new(
           videoId ?? Guid.NewGuid(),
           withVideoFile ? GetValidMediaFileInput() : null,
           withTrailerFile ? GetValidMediaFileInput() : null);

    public UseCase.UploadMedias CreateUseCase()
        => new(
            Mock.Of<IVideoRepository>(),
            Mock.Of<IUnitOfWork>(),
            Mock.Of<IStorageService>()
            );
}
