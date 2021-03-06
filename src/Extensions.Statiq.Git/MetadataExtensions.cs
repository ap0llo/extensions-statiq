﻿using Statiq.Common;

namespace Grynwald.Extensions.Statiq.Git
{
    public static class MetadataExtensions
    {
        /// <summary>
        /// Gets the value for the <see cref="GitKeys.GitRepositoryUrl"/> key.
        /// </summary>
        public static string GetGitRepositoryUrl(this IMetadata metadata) => metadata.GetString(GitKeys.GitRepositoryUrl);

        /// <summary>
        /// Gets the value for the <see cref="GitKeys.GitBranch"/> key.
        /// </summary>
        public static string GetGitBranch(this IMetadata metadata) => metadata.GetString(GitKeys.GitBranch);

        /// <summary>
        /// Gets the value for the <see cref="GitKeys.GitTag"/> key.
        /// </summary>
        public static string GetGitTag(this IMetadata metadata) => metadata.GetString(GitKeys.GitTag);

        /// <summary>
        /// Gets the value for the <see cref="GitKeys.GitCommit"/> key.
        /// </summary>
        public static string GetGitCommit(this IMetadata metadata) => metadata.GetString(GitKeys.GitCommit);

        /// <summary>
        /// Gets the value for the <see cref="GitKeys.GitRelativePath"/> key.
        /// </summary>
        public static NormalizedPath GetGitRelativePath(this IMetadata metadata) => metadata.Get<NormalizedPath>(GitKeys.GitRelativePath);
    }
}
