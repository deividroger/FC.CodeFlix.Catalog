using FC.CodeFlix.Catalog.Domain.Exceptions;
using FluentAssertions;
using System;
using Xunit;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.UnitTests.Domain.Entity.CastMember;

[Collection(nameof(CastMemberTestFixture))]
public class CastMemberTest
{
    private readonly CastMemberTestFixture _fixture;

    public CastMemberTest(CastMemberTestFixture fixture)
        => _fixture = fixture;

    [Fact(DisplayName = nameof(Instantiate))]
    [Trait("Domain", "CastMember - Aggregates")]
    public void Instantiate()
    {
        var name = _fixture.GetValidName();
        var type = _fixture.GetRandomCastMemberType();

        var datetimeBefore = DateTime.Now.AddSeconds(-1);
        var castMember = new DomainEntity.CastMember(name,type);
        var datetimeAfter = DateTime.Now.AddSeconds(1);


        castMember.Id.Should().NotBeEmpty();

        castMember.Name.Should().Be(name);
        castMember.Type.Should().Be(type);
        (castMember.CreatedAt >= datetimeBefore).Should().BeTrue();
        (castMember.CreatedAt <= datetimeAfter).Should().BeTrue();

    }

    [Theory(DisplayName = nameof(ThrowErrorWhenNameIsInvalid))]
    [Trait("Domain", "CastMember - Aggregates")]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData(null)]
    public void ThrowErrorWhenNameIsInvalid(string? name)
    {        
        var type = _fixture.GetRandomCastMemberType();

        var action = () => new DomainEntity.CastMember(name!, type);

        action.Should().Throw<EntityValidationException>()
                        .WithMessage($"Name should not be empty or null");
    }


    [Fact(DisplayName = nameof(Update))]
    [Trait("Domain", "CastMember - Aggregates")]
    public void Update()
    {
        var newName = _fixture.GetValidName();
        var newtype = _fixture.GetRandomCastMemberType();

        
        var castMember = _fixture.GetExampleCastMember();

        castMember.Update(newName, newtype);

        castMember.Name.Should().Be(newName);
        castMember.Type.Should().Be(newtype);
        

    }

    [Theory(DisplayName = nameof(UpateThrowsErrorWhenNameIsInvalid))]
    [Trait("Domain", "CastMember - Aggregates")]
    [InlineData(" ")]
    [InlineData("")]
    [InlineData(null)]
    public void UpateThrowsErrorWhenNameIsInvalid(string? name)
    {
        
        var newtype = _fixture.GetRandomCastMemberType();

        var castMember = _fixture.GetExampleCastMember();

       var action = () =>  castMember.Update(name!, newtype);

        action.Should().Throw<EntityValidationException>()
                        .WithMessage($"Name should not be empty or null");
    }
}