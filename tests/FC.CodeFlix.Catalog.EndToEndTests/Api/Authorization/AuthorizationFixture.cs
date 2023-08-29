using FC.CodeFlix.Catalog.EndToEndTests.Base;
using Xunit;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Authorization;


[CollectionDefinition(nameof(AuthorizationFixture))]
public class AuthorizationFixtureCollection : ICollectionFixture<AuthorizationFixture>
{
}

public class AuthorizationFixture: BaseFixture
{

}
