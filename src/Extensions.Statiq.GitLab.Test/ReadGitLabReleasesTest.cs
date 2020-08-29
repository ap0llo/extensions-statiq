using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GitLabApiClient;
using GitLabApiClient.Internal.Paths;
using GitLabApiClient.Models.Releases.Requests;
using GitLabApiClient.Models.Releases.Responses;
using Grynwald.Extensions.Statiq.TestHelpers;
using Moq;
using NUnit.Framework;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.GitLab.Test
{
    /// <summary>
    /// Tests for <see cref="ReadGitLabReleases"/>
    /// </summary>
    public class ReadGitLabReleasesTest : TestBase
    {
        private Mock<IGitLabClientFactory> m_ClientFactoryMock = null!;     // set in SetUp which is called by NUnit before test execution
        private Mock<IGitLabClient> m_ClientMock = null!;                   // set in SetUp which is called by NUnit before test execution
        private Mock<IReleaseClient> m_ReleaseClientMock = null!;         // set in SetUp which is called by NUnit before test execution

        public override void SetUp()
        {
            base.SetUp();

            m_ClientFactoryMock = new Mock<IGitLabClientFactory>(MockBehavior.Strict);
            m_ClientMock = new Mock<IGitLabClient>(MockBehavior.Strict);
            m_ReleaseClientMock = new Mock<IReleaseClient>(MockBehavior.Strict);

            m_ClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string?>()))
                .Returns(m_ClientMock.Object);

            m_ClientMock
                .Setup(x => x.Releases)
                .Returns(m_ReleaseClientMock.Object);
        }


        [TestCase("namespace", null)]
        [TestCase(null, "projectName")]
        public void Constructor_checks_arguments_for_null(string owner, string repositoryName)
        {
            // ARRANGE
            var ownerConfig = owner == null ? null : Config.FromValue(owner);
            var repositoryNameConfig = repositoryName == null ? null : Config.FromValue(repositoryName);

            Action act = () => new ReadGitLabReleases(ownerConfig!, repositoryNameConfig!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Constructor_ensures_configs_do_not_require_a_document()
        {
            // ARRANGE
            Action act1 = () => new ReadGitLabReleases(Config.FromDocument(d => ""), "project");
            act1.Should().Throw<ArgumentException>();

            Action act2 = () => new ReadGitLabReleases("namespace", Config.FromDocument(d => ""));
            act2.Should().Throw<ArgumentException>();
        }

        [Test]
        public async Task Execute_no_documents_if_there_are_no_GitHub_releases()
        {
            // ARRANGE
            m_ReleaseClientMock
                .Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ReleaseQueryOptions>>()))
                .ReturnsAsync(Array.Empty<Release>());

            var sut = new ReadGitLabReleases("namespace", "project", m_ClientFactoryMock.Object);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().NotBeNull().And.BeEmpty();
        }

        [Test]
        public async Task Execute_returns_expected_documents()
        {
            // ARRANGE
            m_ReleaseClientMock
                .Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ReleaseQueryOptions>>()))
                .ReturnsAsync(new[]
                {
                    new Release()
                    {
                        ReleaseName = "v1.0",
                        TagName =  "some-tag",
                        Description = "Body 1"
                    },
                    new Release()
                    {
                        ReleaseName = "v2.0 Beta",
                        TagName = "some-other-tag",
                        Description = "Body 2"
                    }
                });

            var sut = new ReadGitLabReleases("namespace", "project", m_ClientFactoryMock.Object);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            outputs.Should().NotBeNull().And.HaveCount(2);

            {
                var document = outputs.First();

                document.GetGitLabReleaseName().Should().Be("v1.0");
                document.GetGitLabReleaseTagName().Should().Be("some-tag");
                document.MediaTypeEquals(MediaTypes.Markdown).Should().BeTrue();
                (await document.GetContentStringAsync()).Should().Be("Body 1");
            }
            {
                var document = outputs.Last();

                document.GetGitLabReleaseName().Should().Be("v2.0 Beta");
                document.GetGitLabReleaseTagName().Should().Be("some-other-tag");
                document.MediaTypeEquals(MediaTypes.Markdown).Should().BeTrue();
                (await document.GetContentStringAsync()).Should().Be("Body 2");
            }
        }

        [TestCase(null)]
        [TestCase(630822816000000000L)]
        public async Task Execute_adds_Release_metadata_if_the_release_has_release_base(long? ticks)
        {
            var releasedAt = ticks.HasValue ? new DateTime(ticks.Value).ToUniversalTime() : default(DateTime?);

            // ARRANGE
            m_ReleaseClientMock
                .Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ReleaseQueryOptions>>()))
                .ReturnsAsync(new[]
                {
                    new Release()
                    {
                        ReleasedAt = releasedAt
                    }
                });

            var sut = new ReadGitLabReleases("namespace", "project", m_ClientFactoryMock.Object);

            // ACT
            var output = await ExecuteAsync(sut).SingleAsync();

            // ASSERT
            if (ticks.HasValue)
            {
                output.GetGitLabReleaseDate().Should().Be(releasedAt!.Value);
            }
            else
            {
                output.GetGitLabReleaseDate().Should().BeNull();
            }
        }

        [TestCase(null, "gitlab.com")]
        [TestCase("gitlab.com", "gitlab.com")]
        [TestCase("example.com", "example.com")]
        public async Task Execute_uses_the_configured_host_name(string? configuredHostName, string expectedHostName)
        {
            // ARRANGE
            m_ReleaseClientMock
                .Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ReleaseQueryOptions>>()))
                .ReturnsAsync(Array.Empty<Release>());

            var sut = new ReadGitLabReleases("namespace", "project", m_ClientFactoryMock.Object);

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
            m_ReleaseClientMock
                .Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ReleaseQueryOptions>>()))
                .ReturnsAsync(Array.Empty<Release>());

            var sut = new ReadGitLabReleases("namespace", "project", m_ClientFactoryMock.Object)
                .WithAccessToken(accessToken!);

            // ACT
            var outputs = await ExecuteAsync(sut);

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>(), accessToken), Times.Once);
        }


        [TestCase(null, "markdown", "html", "markdown")]
        [TestCase(OutputFormat.Markdown, "markdown", "html", "markdown")]
        [TestCase(OutputFormat.Html, "markdown", "html", "html")]
        public async Task Execute_returns_releases_in_the_configured_output_format(OutputFormat? format, string markdown, string html, string expectedContent)
        {
            // ARRANGE
            m_ReleaseClientMock
                .Setup(x => x.GetAsync(It.IsAny<ProjectId>(), It.IsAny<Action<ReleaseQueryOptions>>()))
                .ReturnsAsync(new[]
                {
                    new Release()
                    {
                        Description = markdown,
                        DescriptionHtml = html
                    }
                });


            var sut = new ReadGitLabReleases("namespace", "project", m_ClientFactoryMock.Object);

            if (format.HasValue)
            {
                sut = sut.WithOutputFormat(format.Value);
            }

            var expectedMediaType = format switch
            {
                OutputFormat.Html => MediaTypes.Html,
                OutputFormat.Markdown => MediaTypes.Markdown,
                _ => MediaTypes.Markdown
            };

            // ACT
            var output = await ExecuteAsync(sut).SingleAsync();

            // ASSERT
            var actualContent = await output.GetContentStringAsync();

            actualContent.Should().Be(expectedContent);
            output.MediaTypeEquals(expectedMediaType).Should().BeTrue();
        }
    }
}
