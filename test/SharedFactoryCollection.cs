using Xunit;
using Chirp.Tests;

namespace Chirp.SharedFactory;

[CollectionDefinition("SharedFactory")]
public class SharedFactoryCollection : ICollectionFixture<WebAppFactory>
{
}
