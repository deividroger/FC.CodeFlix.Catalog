
using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.Domain.Extensions;
using FluentAssertions;
using System;
using Xunit;

namespace FC.CodeFlix.Catalog.UnitTests.Domain.Extensions;

public class RatingExtensionsTest
{
    [Theory(DisplayName =nameof(StringToRating))]
    [Trait("Domain","Rating - Extensions")]
    [InlineData("ER",Rating.ER)]
    [InlineData("L", Rating.L)]
    [InlineData("10", Rating.Rate10)]
    [InlineData("12", Rating.Rate12)]
    [InlineData("14", Rating.Rate14)]
    [InlineData("16", Rating.Rate16)]
    [InlineData("18", Rating.Rate18)]
    public void StringToRating(string enumString,Rating expectedRating)
        => enumString.ToRating().Should().Be(expectedRating);
    
    [Theory(DisplayName = nameof(RatingToString))]
    [Trait("Domain", "Rating - Extensions")]
    [InlineData(Rating.ER, "ER")]
    [InlineData(Rating.L,"L" )]
    [InlineData(Rating.Rate10,"10" )]
    [InlineData(Rating.Rate12,"12")]
    [InlineData(Rating.Rate14,"14")]
    [InlineData(Rating.Rate16,"16" )]
    [InlineData(Rating.Rate18,"18")]
    public void RatingToString(Rating rating, string expectedString)
        => rating.ToStringSignal().Should().Be(expectedString);
    

    [Fact(DisplayName =nameof(ThrowsExceptionWhenInvalidString))]
    [Trait("Domain","Rating - Extensions")]
    public void ThrowsExceptionWhenInvalidString()
    {
        var action = () => "invalid".ToRating();
        action.Should().Throw<ArgumentOutOfRangeException>(); 
    }
}