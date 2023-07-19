
using FC.CodeFlix.Catalog.Application.UseCases.Video.DeleteVideo;
using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using System;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.DeleteVideo;


[CollectionDefinition(nameof(DeleteVideoTestFixture))]
public class DeleteVideoTestFixtureCollection: ICollectionFixture<DeleteVideoTestFixture>
{
}

public class DeleteVideoTestFixture : VideoTestFixtureBase
{
    public DeleteVideoInput GetValidInput(Guid? id = null)
        => new (id ?? Guid.NewGuid());
}
