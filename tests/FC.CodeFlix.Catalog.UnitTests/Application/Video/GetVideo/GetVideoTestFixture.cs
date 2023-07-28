using FC.CodeFlix.Catalog.UnitTests.Common.Fixtures;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Video.GetVideo
{

    [CollectionDefinition(nameof(GetVideoTestFixture))]
    public class CreateVideoTestFixtureCollection: ICollectionFixture<GetVideoTestFixture>
    {

    }

    public class GetVideoTestFixture: VideoTestFixtureBase
    {
    }
}
