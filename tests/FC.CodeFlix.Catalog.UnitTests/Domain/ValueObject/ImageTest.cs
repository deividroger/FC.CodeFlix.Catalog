using FC.CodeFlix.Catalog.UnitTests.Common;
using FC.CodeFlix.Catalog.Domain.ValueObject;
using Xunit;
using FluentAssertions;

namespace FC.CodeFlix.Catalog.UnitTests.Domain.ValueObject;

public class ImageTest: BaseFixture
{
    [Fact(DisplayName =nameof(Instantiate))]
    [Trait("Domain", "Image - Value Objects")]
    public void Instantiate()
    {
        var path = Faker.Image.PicsumUrl();

        var image = new Image(path);

        image.Path.Should().Be(path);   
    }

    [Fact(DisplayName = nameof(EqualsByPath))]
    [Trait("Domain", "Image - Value Objects")]
    public void EqualsByPath()
    {
        var path = Faker.Image.PicsumUrl();
        var image = new Image(path);
        var sameImage = new Image(path);

        var isEquals = image == sameImage;
            
        isEquals.Should().BeTrue(); 
    }


    [Fact(DisplayName = nameof(DifferentByPath))]
    [Trait("Domain", "Image - Value Objects")]
    public void DifferentByPath()
    {
        var path = Faker.Image.PicsumUrl();
        var path2 = Faker.Image.PicsumUrl();
        var image = new Image(path);
        var sameImage = new Image(path2);

        var isDifferent = image != sameImage;

        isDifferent.Should().BeTrue();
    }
}
