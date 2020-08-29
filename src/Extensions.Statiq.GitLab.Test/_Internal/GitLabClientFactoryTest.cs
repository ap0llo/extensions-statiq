// Note: Adapted fromhttps://github.com/ap0llo/changelog/blob/ca85f816078dedbed1bb93e66096796a44e89e0e/src/ChangeLog.Test/Integrations/GitLab/GitLabClientFactoryTest.cs

using FluentAssertions;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.GitLab.Test
{
    public class GitLabClientFactoryTest
    {
        [TestCase("gitlab.com", "https://gitlab.com/api/v4/")]
        [TestCase("example.com", "https://example.com/api/v4/")]
        public void GetClient_returns_the_expected_client(string hostName, string expectedHostUrl)
        {
            // ARRANGE
            var sut = new GitLabClientFactory();

            // ACT
            var client = sut.CreateClient(hostName, null);

            // ASSERT
            client.HostUrl.Should().Be(expectedHostUrl);
        }

        [Test]
        public void CreateClient_succeeds_if_an_access_token_is_supplies()
        {
            // ARRANGE
            var sut = new GitLabClientFactory();

            // ACT
            var client = sut.CreateClient("gitlab.com", accessToken: "00000000000000000000");

            // ASSERT
            Assert.NotNull(client);
        }

        [Theory]
        [TestCase("")]
        [TestCase(null)]
        public void CreateClient_succeeds_if_no_access_token_is_configured(string accessToken)
        {
            // ARRANGE            
            var sut = new GitLabClientFactory();

            // ACT
            var client = sut.CreateClient("gitlab.com", accessToken);

            // ASSERT
            Assert.NotNull(client);
        }
    }
}
