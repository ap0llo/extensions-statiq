using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grynwald.Extensions.Statiq.TestHelpers;
using Moq;
using NUnit.Framework;
using Octokit;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.GitHub.Test
{
    /// <summary>
    /// Tests for <see cref="ReadGitHubReleases"/>
    /// </summary>
    public class ReadGitHubReleasesTest : TestBase
    {
        private Mock<IGitHubClientFactory> m_ClientFactoryMock = null!;     // set in SetUp which is called by NUnit before test execution
        private Mock<IGitHubClient> m_ClientMock = null!;                   // set in SetUp which is called by NUnit before test execution
        private Mock<IRepositoriesClient> m_RepositoriesClientMock = null!; // set in SetUp which is called by NUnit before test execution
        private Mock<IReleasesClient> m_ReleasesClientMock = null!;         // set in SetUp which is called by NUnit before test execution

        public override void SetUp()
        {
            base.SetUp();

            m_ClientFactoryMock = new Mock<IGitHubClientFactory>(MockBehavior.Strict);
            m_ClientMock = new Mock<IGitHubClient>(MockBehavior.Strict);
            m_RepositoriesClientMock = new Mock<IRepositoriesClient>(MockBehavior.Strict);
            m_ReleasesClientMock = new Mock<IReleasesClient>(MockBehavior.Strict);

            m_ClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(m_ClientMock.Object);

            m_ClientMock
                .Setup(x => x.Repository)
                .Returns(m_RepositoriesClientMock.Object);

            m_RepositoriesClientMock
                .Setup(x => x.Release)
                .Returns(m_ReleasesClientMock.Object);
        }

        private Release CreateRelease(
            string url = "https://example.com",
            string htmlUrl = "https://example.com/html",
            string assetsUrl = "https://example.com/assets",
            string uploadUrl = "https://example.com/upload",
            string tagName = "tagName",
            string commitId = "baadf00d",
            string name = "releaseName",
            string body = "",
            bool draft = false,
            bool prerelease = false,
            DateTimeOffset? publishedAt = null)
        {
            return new Release(
                url: url,
                htmlUrl: htmlUrl,
                assetsUrl,
                uploadUrl,
                id: 0,
                nodeId: "",
                tagName: tagName,
                targetCommitish: commitId,
                name: name,
                body: body,
                draft: draft,
                prerelease: prerelease,
                createdAt: DateTimeOffset.Now,
                publishedAt: publishedAt,
                author: new Author(),
                tarballUrl: null,
                zipballUrl: null,
                assets: Array.Empty<ReleaseAsset>()
            );
        }

        [TestCase("owner", null)]
        [TestCase(null, "repositoryName")]
        public void Constructor_checks_arguments_for_null(string owner, string repositoryName)
        {
            // ARRANGE
            var ownerConfig = owner == null ? null : Config.FromValue(owner);
            var repositoryNameConfig = repositoryName == null ? null : Config.FromValue(repositoryName);

            Action act = () => new ReadGitHubReleases(ownerConfig!, repositoryNameConfig!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ensures_configs_do_not_require_a_document()
        {
            // ARRANGE
            Action act1 = () => new ReadGitHubReleases(Config.FromDocument(d => ""), "repo");
            act1.Should().Throw<ArgumentException>();

            Action act2 = () => new ReadGitHubReleases("owner", Config.FromDocument(d => ""));
            act2.Should().Throw<ArgumentException>();
        }

        [Test]
        public async Task Execute_no_documents_if_there_are_no_GitHub_releases()
        {
            // ARRANGE
            m_ReleasesClientMock
                .Setup(x => x.GetAll("owner", "repo"))
                .ReturnsAsync(Array.Empty<Release>());

            var sut = new ReadGitHubReleases("owner", "repo", m_ClientFactoryMock.Object);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public async Task Execute_returns_expected_documents()
        {
            // ARRANGE
            m_ReleasesClientMock
                .Setup(x => x.GetAll("owner", "repo"))
                .ReturnsAsync(new[]
                {
                    CreateRelease(
                        name: "v1.0",
                        draft: true,
                        prerelease: false,
                        tagName: "some-tag",
                        htmlUrl: "http://example.com/releases/v1.0",
                        body: "Body 1"
                    ),
                    CreateRelease(
                        name: "v2.0 Beta",
                        draft: false,
                        prerelease: true,
                        tagName: "some-other-tag",
                        htmlUrl: null!,
                        body: "Body 2"
                    )
                });

            var sut = new ReadGitHubReleases("owner", "repo", m_ClientFactoryMock.Object);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().NotBeNull().And.HaveCount(2);

            {
                var document = outputs.First();

                document.GetGitHubReleaseName().Should().Be("v1.0");
                document.GetGitHubReleaseIsDraft().Should().BeTrue();
                document.GetGitHubReleaseIsPrerelease().Should().BeFalse();
                document.GetGitHubReleaseTagName().Should().Be("some-tag");
                document.GetGitHubHtmlUrl().Should().Be("http://example.com/releases/v1.0");
                document.MediaTypeEquals(MediaTypes.Markdown).Should().BeTrue();
                (await document.GetContentStringAsync()).Should().Be("Body 1");
            }
            {
                var document = outputs.Last();

                document.GetGitHubReleaseName().Should().Be("v2.0 Beta");
                document.GetGitHubReleaseIsDraft().Should().BeFalse();
                document.GetGitHubReleaseIsPrerelease().Should().BeTrue();
                document.GetGitHubReleaseTagName().Should().Be("some-other-tag");
                document.GetGitHubHtmlUrl().Should().BeNull();
                document.MediaTypeEquals(MediaTypes.Markdown).Should().BeTrue();
                (await document.GetContentStringAsync()).Should().Be("Body 2");
            }
        }

        [TestCase(null)]
        [TestCase(630822816000000000L)]
        public async Task Execute_adds_PublishedAt_metadata_when_the_release_is_published(long? ticks)
        {
            var publishedAt = ticks.HasValue ? new DateTimeOffset(ticks.Value, TimeSpan.Zero) : default(DateTimeOffset?);

            // ARRANGE
            m_ReleasesClientMock
                .Setup(x => x.GetAll("owner", "repo"))
                .ReturnsAsync(new[]
                {
                    CreateRelease(publishedAt: publishedAt)
                });

            var sut = new ReadGitHubReleases("owner", "repo", m_ClientFactoryMock.Object);

            // ACT
            var output = await ExecuteAsync(sut).SingleAsync();

            // ASSERT
            if (ticks.HasValue)
            {
                output.GetGitHubReleasePublishedAt().Should().Be(publishedAt!.Value.UtcDateTime);
            }
            else
            {
                output.GetGitHubReleasePublishedAt().Should().BeNull();
            }
        }

        [TestCase(null, "github.com")]
        [TestCase("github.com", "github.com")]
        [TestCase("example.com", "example.com")]
        public async Task Execute_uses_the_configured_host_name(string? configuredHostName, string expectedHostName)
        {
            // ARRANGE
            m_ReleasesClientMock
                .Setup(x => x.GetAll("owner", "repo"))
                .ReturnsAsync(Array.Empty<Release>());

            var sut = new ReadGitHubReleases("owner", "repo", m_ClientFactoryMock.Object);

            if (configuredHostName != null)
            {
                sut = sut.WithHostName(configuredHostName);
            }

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(expectedHostName, It.IsAny<string?>()), Times.Once);
        }

        [TestCase(null)]
        [TestCase("some-access-token")]
        public async Task Execute_uses_the_configured_access_token(string? accessToken)
        {
            // ARRANGE
            m_ReleasesClientMock
                .Setup(x => x.GetAll("owner", "repo"))
                .ReturnsAsync(Array.Empty<Release>());

            var sut = new ReadGitHubReleases("owner", "repo", m_ClientFactoryMock.Object)
                .WithAccessToken(accessToken!);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>(), accessToken), Times.Once);
        }
    }
}
