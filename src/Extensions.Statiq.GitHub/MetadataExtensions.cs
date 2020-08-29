using System;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.GitHub
{
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets <see cref="GitHubKeys.GitHubReleaseName"/> metadata value.
        /// </summary>
        public static string GetGitHubReleaseName(this IMetadata metadata) => metadata.GetString(GitHubKeys.GitHubReleaseName);

        /// <summary>
        /// Gets <see cref="GitHubKeys.GitHubReleasePublishedAt"/> metadata value.
        /// </summary>
        public static DateTime? GetGitHubReleasePublishedAt(this IMetadata metadata) => metadata.Get<DateTime?>(GitHubKeys.GitHubReleasePublishedAt);

        /// <summary>
        /// Gets <see cref="GitHubKeys.GitHubReleaseIsDraft"/> metadata value.
        /// </summary>
        public static bool GetGitHubReleaseIsDraft(this IMetadata metadata) => metadata.GetBool(GitHubKeys.GitHubReleaseIsDraft);

        /// <summary>
        /// Gets <see cref="GitHubKeys.GitHubReleaseIsPrerelease"/> metadata value.
        /// </summary>
        public static bool GetGitHubReleaseIsPrerelease(this IMetadata metadata) => metadata.GetBool(GitHubKeys.GitHubReleaseIsPrerelease);

        /// <summary>
        /// Gets <see cref="GitHubKeys.GitHubReleaseTagName"/> metadata value.
        /// </summary>
        public static string GetGitHubReleaseTagName(this IMetadata metadata) => metadata.GetString(GitHubKeys.GitHubReleaseTagName);

        /// <summary>
        /// Gets <see cref="GitHubKeys.GitHubHtmlUrl"/> metadata value.
        /// </summary>
        public static string GetGitHubHtmlUrl(this IMetadata metadata) => metadata.GetString(GitHubKeys.GitHubHtmlUrl);

    }
}
