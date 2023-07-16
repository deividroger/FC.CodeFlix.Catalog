using FC.CodeFlix.Catalog.Application.Common;
using FluentAssertions;
using System;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Application.Common;

public class StorageFileNameTest
{
    [Fact(DisplayName = nameof(CreateStorageNameForFile))]
    [Trait("Application", "StorageName - Common")]
    public void CreateStorageNameForFile()
    {
        var exampleId = Guid.NewGuid();
        var exampleExtension = "mp4";
        var propertyName = "Video";

        var name = StorageFileName.Create(exampleId, propertyName, exampleExtension);

        name.Should().Be($"{exampleId}-{propertyName.ToLower()}.{exampleExtension}");

    }
}
