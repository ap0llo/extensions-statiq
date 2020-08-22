using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Utilities;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Globber = Grynwald.Extensions.Statiq.Git.Internal.Globber;

namespace Grynwald.Extensions.Statiq.Git
{
    /// <summary>
    /// Reads files from a git repository (from one or more branches)
    /// </summary>
    /// <remarks>
    /// This modules allows reading a file from a git repository.
    /// <para>
    /// The url of the git repository can be either a local path or a http(s) url for a remote repository.
    /// If the url of a remote repository is specified, the repository will be cloned to a temporary folder.
    /// Note that the files are read directory from the git repository (not the working copy),
    /// so uncommited changes in a local repository will not be read.
    /// </para>
    /// <para>
    /// By default, all files from a repository's branch are read.
    /// To limit the set of files, specify one or more file patterns using either the constructor or <see cref="WithFilePatterns(String[])"/>
    /// The file pattern format is the same ´format used by the <c>ReadFiles</c> module in Statiq.Core.
    /// </para>
    /// <para>
    /// By default, data will be read from the repository's default branch.
    /// To specify a different branch (or multiple branches), use <see cref="WithBranchNames(String[])"/>.
    /// </para>
    /// <para>
    /// The module will add "placeholder" paths for the document's <see cref="IDocument.Source"/> and <see cref="IDocument.Destination"/> properties.
    /// The placeholder destination path uses the format <c>git/REPOURLHASH/BRANCHNAME/RELATIVEPATH</c>:
    /// <list type="bullet">
    ///     <item>
    ///         <term><c>REPOURLHASH</c></term>
    ///         <description>The SHA1-hash of the repository url the file was read from.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>BRANCHNAME</c></term>
    ///         <description>The name of the branch the document was read from.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>RELATIVEPATH</c></term>
    ///         <description>The relative path of the file within the repository the document was read from.</description>
    ///     </item>
    /// </list>
    /// For the value of <see cref="IDocument.Source"/>, a root directory is added to the destination path to make the path absolute.
    /// While this means the documents have a source path that does not actually exist on disk, this allows modules to calculate relative paths
    /// between the source files which correspond to relative paths in the input repository (e.g. to resolve links between markdown documents).
    /// </para>
    /// <para>
    /// For each documents, the module set the following metadata keys:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="GitKeys.GitRepositoryUrl"/></term>
    ///         <description>The url of the repository the document were read from.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitKeys.GitBranch"/></term>
    ///         <description>The name of the branch the document was read from.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitKeys.GitCommit"/></term>
    ///         <description>The git commit id of the commit the document was read from.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="GitKeys.GitRelativePath"/></term>
    ///         <description>The relative path in the git repository of the input document (as <see cref="NormalizedPath"/>).</description>
    ///     </item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <example>
    /// To read all files from the <c>docs</c> folder from the <c>main</c> branch and all branches which name starts with <c>release/</c>, use
    /// <code lang="cs">
    /// new ReadFilesFromGit("https://example.com/my-repository.git", "docs/**")
    ///     .WithBranchPatterns("main", "release/*")
    /// </code>
    /// </example>
    public sealed class ReadFilesFromGit : Module
    {
        private readonly Config<string> m_RepositoryUrl;
        private IReadOnlyList<string>? m_BranchNames = null;
        private IReadOnlyList<string> m_FilePatterns;


        /// <summary>
        /// Reads all files from a git repository.
        /// </summary>
        /// <param name="repositoryUrl">The url of the git repository. This can be a local path or a http(s) url.</param>
        public ReadFilesFromGit(string repositoryUrl) : this(Config.FromValue(repositoryUrl))
        { }

        /// <inheritdoc cref="ReadFilesFromGit(String)"/>
        public ReadFilesFromGit(Config<string> repositoryUrl)
        {
            m_RepositoryUrl = repositoryUrl ?? throw new ArgumentNullException(nameof(repositoryUrl));
            m_FilePatterns = new[] { "**" };
        }

        /// <summary>
        /// Reads files from a git repository.
        /// </summary>
        /// <param name="repositoryUrl">The url of the git repository. This can be a local path or a http(s) url.</param>
        /// <param name="filePatterns">One or more patterns to use for selecting files in the git repository.</param>
        public ReadFilesFromGit(string repositoryUrl, params string[] filePatterns) : this(Config.FromValue(repositoryUrl), filePatterns)
        { }

        /// <inheritdoc cref="ReadFilesFromGit(String, String[])"/>
        public ReadFilesFromGit(Config<string> repositoryUrl, params string[] filePatterns)
        {
            m_RepositoryUrl = repositoryUrl ?? throw new ArgumentNullException(nameof(repositoryUrl));
            m_FilePatterns = filePatterns.ToArray();
        }

        /// <summary>
        /// Sets the names of the branches to read from the repository.
        /// </summary>
        /// <remarks>
        /// By default, documents are read from the repository's default branch.
        /// Supports wildcards, (e.g. <c>release/*</c>) to read branches from all branches matching the pattern.
        /// </remarks>
        public ReadFilesFromGit WithBranchNames(params string[] branchNames)
        {
            m_BranchNames = branchNames.ToArray();
            return this;
        }

        /// <summary>
        /// Sets the patterns to use for filtering which files to read from the repository.
        /// </summary>
        /// <remarks>
        /// Uses the same format as <c>ReadFiles</c> module in Statiq.Core.
        /// </remarks>
        public ReadFilesFromGit WithFileNames(params string[] filePatterns)
        {
            m_FilePatterns = filePatterns.ToArray();
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            m_RepositoryUrl.EnsureNonDocument();
            var repositoryUrl = await m_RepositoryUrl.GetValueAsync(null, context);

            var repositoryUrlHash = repositoryUrl.ComputeHashString();

            // load repository
            using var repository = GitRepositoryFactory.GetRepository(repositoryUrl);

            IReadOnlyList<string> matchingBranches;
            if (m_BranchNames is null)
            {
                matchingBranches = new[] { repository.CurrentBranch };
            }
            else
            {
                var branchPatterns = m_BranchNames.Select(x => new Wildcard(x)).ToList();

                // get branches matching the patterns
                matchingBranches = repository.Branches
                  .Where(branchName => branchPatterns.Any(pattern => pattern.IsMatch(branchName)))
                  .ToList();

                if (!matchingBranches.Any())
                {
                    context.LogWarning($"The repository contains no branches matching any of the configured names {String.Join(", ", m_BranchNames.Select(x => $"'{x}'"))}");
                }
            }

            if (!matchingBranches.Any())
            {
                return Enumerable.Empty<IDocument>();
            }

            var outputs = new List<IDocument>();

            // for each branch, read documents and add them to the output.
            foreach (var branchName in matchingBranches)
            {
                var commitId = repository.GetHeadCommitId(branchName);
                var rootDir = repository.GetRootDirectory(commitId);

                var branchOutputs = Globber.GetFiles(rootDir, m_FilePatterns).Select(file =>
                {
                    var placeholderDestinationPath = new NormalizedPath(repositoryUrlHash).Combine(branchName).Combine(file.FullName);
                    var placeholderSourcePath = context.FileSystem.RootPath
                        .Combine("git")
                        .Combine(placeholderDestinationPath);

                    var metadata = new Dictionary<string, object>()
                    {
                        { GitKeys.GitRepositoryUrl, repositoryUrl },
                        { GitKeys.GitBranch, branchName },
                        { GitKeys.GitCommit, commitId.ToString() },
                        { GitKeys.GitRelativePath, new NormalizedPath(file.FullName) },
                    };

                    using var stream = file.GetContentStream();
                    return context.CreateDocument(
                        source: placeholderSourcePath,
                        destination: placeholderDestinationPath,
                        stream: stream)
                    .Clone(metadata);
                });

                outputs.AddRange(branchOutputs);
            }

            return outputs;
        }
    }
}
