using System;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.GitLab
{
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets <see cref="GitLabKeys.GitLabReleaseName"/> metadata value.
        /// </summary>
        public static string GetGitLabReleaseName(this IMetadata metadata) => metadata.GetString(GitLabKeys.GitLabReleaseName);

        /// <summary>
        /// Gets <see cref="GitLabKeys.GitLabReleaseDate"/> metadata value.
        /// </summary>
        public static DateTime? GetGitLabReleaseDate(this IMetadata metadata) => metadata.Get<DateTime?>(GitLabKeys.GitLabReleaseDate);

        /// <summary>
        /// Gets <see cref="GitLabKeys.GitLabReleaseTagName"/> metadata value.
        /// </summary>
        public static string GetGitLabReleaseTagName(this IMetadata metadata) => metadata.GetString(GitLabKeys.GitLabReleaseTagName);
    }
}
