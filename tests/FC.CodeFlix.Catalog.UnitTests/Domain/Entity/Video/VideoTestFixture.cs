using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Domain.Entity.Video;

[CollectionDefinition(nameof(VideoTestFixture))]
public class VideoTestFixtureCollection : ICollectionFixture<VideoTestFixture>
{
}

public class VideoTestFixture : VideoBaseFixture
{
 
}
