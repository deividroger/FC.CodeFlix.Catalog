using FC.CodeFlix.Catalog.UnitTests.Common;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Infra.Storage;

[CollectionDefinition(nameof(StorageServiceTestFixture))]
public class StorageServiceTestFixtureCollection
    : ICollectionFixture<StorageServiceTestFixture>
{

}


public class StorageServiceTestFixture: BaseFixture
{
    public  string GetBucketName()
        => "fc3-catalogs-medias";

    public string GetFileName()
        => Faker!.System.CommonFileName();

    public string GetContentFile()
        => Faker!.Lorem.Paragraph(3);

    public string GetContentType()
        => Faker!.System.MimeType();
}
