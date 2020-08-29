using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GitLabApiClient.Internal.Paths;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.GitLab
{
    /// <summary>
    /// Reads a project's releases from GitLab and outputs them as documents. 
    /// </summary>
    /// <remarks>
    /// The module outputs one document for every GitLab release that is found.
    /// The output documents' content is the body of the GitLab releases as Markdown (default) or HTML.
    /// <para>
    /// In addition to the content, the following metadata is set:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="GitLabKeys.GitLabReleaseName"/></term>
    ///         <description>The name of the release</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitLabKeys.GitLabReleaseTagName"/></term>
    ///         <description>The name of the tag the release refers to.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitLabKeys.GitLabReleaseDate"/></term>
    ///         <description>
    ///         The date the release was published as <see cref="DateTime?"/>.
    ///         The metadata value is not added if the release is not yet published.
    ///         </description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class ReadGitLabReleases : Module
    {
        public const string DefaultHostName = "gitlab.com";

        private IGitLabClientFactory? m_GitLabClientFactory;
        private readonly Config<string> m_ProjectNamespace;
        private readonly Config<string> m_ProjectName;
        private Config<string>? m_AccessToken;
        private Config<string> m_HostName = Config.FromValue(DefaultHostName);
        private Config<OutputFormat>? m_OutputFormat;

        public OutputFormat OutputFormat { get; private set; }


        /// <summary>
        /// Reads a project's releases from GitLab and outputs them as documents. 
        /// </summary>
        /// <param name="projectNamespace">The project namespace (user/group) of the project to get releases for.</param>
        /// <param name="projectName">The name of the project to get releases for.</param>
        public ReadGitLabReleases(Config<string> projectNamespace, Config<string> projectName) : this(projectNamespace, projectName, null)
        { }

        internal ReadGitLabReleases(Config<string> projectNamespace, Config<string> projectName, IGitLabClientFactory? gitlabClientFactory)
        {
            projectNamespace.EnsureNonDocument();
            projectName.EnsureNonDocument();

            m_ProjectNamespace = projectNamespace;
            m_ProjectName = projectName;
            m_GitLabClientFactory = gitlabClientFactory;
        }


        /// <summary>
        /// Sets the access token to use for requesting data from the GitLab API.
        /// To send unauthenticated requests, set access token to <c>null</c>
        /// </summary>
        public ReadGitLabReleases WithAccessToken(Config<string>? accessToken)
        {
            accessToken?.EnsureNonDocument();
            m_AccessToken = accessToken;
            return this;
        }

        /// <summary>
        /// Sets the host name of the GitLab server to request data from.
        /// By default, <c>gitlab.com</c> will be used.
        /// </summary>
        public ReadGitLabReleases WithHostName(Config<string> hostName)
        {
            hostName.EnsureNonDocument();
            m_HostName = hostName;
            return this;
        }


        /// <summary>
        /// Sets the output format for releases read from Gitlab
        /// </summary>
        public ReadGitLabReleases WithOutputFormat(Config<OutputFormat>? outputFormat)
        {
            outputFormat?.EnsureNonDocument();
            m_OutputFormat = outputFormat;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            var clientFactory = m_GitLabClientFactory ?? new GitLabClientFactory();

            var hostName = await m_HostName.GetValueAsync(null, context);
            var projectNamespace = await m_ProjectNamespace.GetValueAsync(null, context);
            var projectName = await m_ProjectName.GetValueAsync(null, context);
            var accessToken = m_AccessToken == null ? null : await m_AccessToken.GetValueAsync(null, context);
            var outputFormat = m_OutputFormat == null ? default(OutputFormat?) : await m_OutputFormat.GetValueAsync(null, context);

            var client = clientFactory.CreateClient(hostName, accessToken);

            var releases = await client.Releases.GetAsync((ProjectId)$"{projectNamespace}/{projectName}");

            return await releases.ParallelSelectAsync(async release =>
            {
                var metadata = new Dictionary<string, object>()
                {
                    { GitLabKeys.GitLabReleaseName, release.ReleaseName },
                    { GitLabKeys.GitLabReleaseTagName, release.TagName },
                };

                if (release.ReleasedAt.HasValue)
                {
                    metadata.Add(GitLabKeys.GitLabReleaseDate, release.ReleasedAt.Value.ToUniversalTime());
                }

                var contentProvider = outputFormat == OutputFormat.Html
                    ? await context.GetContentProviderAsync(content: release.DescriptionHtml, mediaType: MediaTypes.Html)
                    : await context.GetContentProviderAsync(content: release.Description, mediaType: MediaTypes.Markdown);

                return context.CreateDocument(metadata, contentProvider);
            });
        }
    }
}
