using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octokit;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.GitHub
{
    /// <summary>
    /// Reads a repository's releases from GitHub and outputs them as documents. 
    /// </summary>
    /// <remarks>
    /// The module outputs one document for every GitHub release that is found.
    /// The output documents' content is the body of the GitHub releases as Markdown.
    /// <para>
    /// In addition to the content, the following metadata is set:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="GitHubKeys.GitHubReleaseName"/></term>
    ///         <description>The name of the release</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitHubKeys.GitHubReleaseIsDraft"/></term>
    ///         <description>A boolean value indicating if the release is a draft release</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitHubKeys.GitHubReleaseIsPrerelease"/></term>
    ///         <description>A boolean value indicating if the release is a marked as prerelease.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitHubKeys.GitHubReleaseTagName"/></term>
    ///         <description>The name of the tag the GitHub release refers to.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitHubKeys.GitHubHtmlUrl"/></term>
    ///         <description>The url of the Release in the GitHub web interface.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitHubKeys.GitHubReleasePublishedAt"/></term>
    ///         <description>
    ///         The date the release was published as <see cref="DateTime?"/>.
    ///         The metadata value is not added if the release is not yet published.
    ///         </description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class ReadGitHubReleases : Module
    {
        public const string DefaultHostName = "github.com";

        private readonly IGitHubClientFactory? m_GitHubClientFactory;
        private readonly Config<string> m_Owner;
        private readonly Config<string> m_RepositoryName;
        private Config<string>? m_AccessToken;
        private Config<string> m_HostName = Config.FromValue(DefaultHostName);

        /// <summary>
        /// Reads a repository's releases from GitHub and outputs them as documents.
        /// </summary>
        /// <param name="owner">The owner (user/organization) of the repository to get releases for.</param>
        /// <param name="repositoryName">The name of the repository to get releases for.</param>
        public ReadGitHubReleases(Config<string> owner, Config<string> repositoryName) : this(owner, repositoryName, null)
        { }

        internal ReadGitHubReleases(Config<string> owner, Config<string> repositoryName, IGitHubClientFactory? githubClientFactory)
        {
            owner.EnsureNonDocument();
            repositoryName.EnsureNonDocument();

            m_Owner = owner;
            m_RepositoryName = repositoryName;
            m_GitHubClientFactory = githubClientFactory;
        }

        /// <summary>
        /// Sets the GitHub access token to use for requesting data from the GitHub API.
        /// To send unauthenticated requests, set access token to <c>null</c>
        /// </summary>
        public ReadGitHubReleases WithAccessToken(Config<string>? accessToken)
        {
            accessToken?.EnsureNonDocument();
            m_AccessToken = accessToken;
            return this;
        }

        /// <summary>
        /// Sets the host name of the GitHub server to request data from (when using a GitHub Enterprise server).
        /// By default, <c>github.com</c> will be used.
        /// </summary>
        public ReadGitHubReleases WithHostName(Config<string> hostName)
        {
            hostName.EnsureNonDocument();
            m_HostName = hostName;
            return this;
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var clientFactory = m_GitHubClientFactory ?? new GitHubClientFactory();

            var hostName = await m_HostName.GetValueAsync(null, context);
            var owner = await m_Owner.GetValueAsync(null, context);
            var repositoryName = await m_RepositoryName.GetValueAsync(null, context);
            var accessToken = m_AccessToken == null ? null : await m_AccessToken.GetValueAsync(null, context);

            var client = clientFactory.CreateClient(hostName, accessToken);

            var releases = await client.Repository.Release.GetAll(owner, repositoryName);


            return await releases.ParallelSelectAsync(async release =>
            {
                var metadata = new Dictionary<string, object>()
                {
                    { GitHubKeys.GitHubReleaseName, release.Name },
                    { GitHubKeys.GitHubReleaseIsDraft, release.Draft },
                    { GitHubKeys.GitHubReleaseIsPrerelease, release.Prerelease },
                    { GitHubKeys.GitHubReleaseTagName, release.TagName },
                    { GitHubKeys.GitHubHtmlUrl, release.HtmlUrl },
                };

                if (release.PublishedAt.HasValue)
                {
                    metadata.Add(GitHubKeys.GitHubReleasePublishedAt, release.PublishedAt.Value.UtcDateTime);
                }

                var contentProvider = await context.GetContentProviderAsync(content: release.Body, mediaType: MediaTypes.Markdown);
                return context.CreateDocument(metadata, contentProvider);
            });
        }
    }
}
