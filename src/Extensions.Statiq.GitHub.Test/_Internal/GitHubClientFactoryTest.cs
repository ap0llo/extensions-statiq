// Note: Adapted from https://github.com/ap0llo/changelog/blob/ca85f816078dedbed1bb93e66096796a44e89e0e/src/ChangeLog.Test/Integrations/GitHub/GitHubClientFactoryTest.cs

using System;
using FluentAssertions;
using NUnit.Framework;
using Octokit;

namespace Grynwald.Extensions.Statiq.GitHub.Test
{
    public class GitHubClientFactoryTest
    {
        [TestCase("github.com", "https://api.github.com/")]
        [TestCase("api.github.com", "https://api.github.com/")]
        [TestCase("github.myenterprise.com", "https://github.myenterprise.com/api/v3/")]
        public void GetClient_returns_the_expected_client(string hostName, string expectedBaseAddress)
        {
            // ARRANGE
            var sut = new GitHubClientFactory();

            // ACT
            var client = sut.CreateClient(hostName, null);

            // ASSERT
            client.Connection.BaseAddress.Should().Be(new Uri(expectedBaseAddress));
        }

        [Test]
        public void CreateClient_adds_an_access_token_if_token_if_a_value_is_supplied()
        {
            // ARRANGE            
            var sut = new GitHubClientFactory();

            // ACT
            var client = sut.CreateClient("github.com", "some-access-token");

            // ASSERT
            var clientConcrete = client
                .Should().NotBeNull()
                .And.BeAssignableTo<GitHubClient>()
                .Which;

            clientConcrete.Credentials.AuthenticationType.Should().Be(AuthenticationType.Oauth);
            clientConcrete.Credentials.Login.Should().BeNull();
            clientConcrete.Credentials.Password.Should().Be("some-access-token");
        }

        [TestCase("")]
        [TestCase(null)]
        public void CreateClient_succeeds_if_no_access_token_is_supplied(string accessToken)
        {
            // ARRANGE
            var sut = new GitHubClientFactory();

            // ACT
            var client = sut.CreateClient("github.com", accessToken);

            // ASSERT
            var clientConcrete = client
                .Should().NotBeNull()
                .And.BeAssignableTo<GitHubClient>()
                .Which;

            clientConcrete.Credentials.AuthenticationType.Should().Be(AuthenticationType.Anonymous);
            clientConcrete.Credentials.Login.Should().BeNull();
            clientConcrete.Credentials.Password.Should().BeNull();
        }

    }
}
