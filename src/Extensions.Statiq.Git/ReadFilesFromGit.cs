using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Utilities;
using Statiq.Common;
using Globber = Grynwald.Extensions.Statiq.Git.Internal.Globber;

namespace Grynwald.Extensions.Statiq.Git
{
    //TODO: Remove hardcoded "master" branch, use repository default branch
    public class ReadFilesFromGit : Module
    {
        private readonly Config<string> m_RepositoryUrl;
        private IReadOnlyList<string> m_BranchPatterns = new[] { "master" };
        private readonly IReadOnlyList<string> m_FilePatterns;


        public ReadFilesFromGit(string repositoryUrl, params string[] filePatterns) : this(Config.FromValue(repositoryUrl), filePatterns)
        { }

        public ReadFilesFromGit(Config<string> repositoryUrl, params string[] filePatterns)
        {
            m_RepositoryUrl = repositoryUrl ?? throw new ArgumentNullException(nameof(repositoryUrl));
            m_FilePatterns = filePatterns.ToArray();
        }


        public ReadFilesFromGit WithBranchPatterns(params string[] branchPatterns)
        {
            m_BranchPatterns = branchPatterns.ToArray();
            return this;
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            m_RepositoryUrl.EnsureNonDocument();

            var repositoryUrl = await m_RepositoryUrl.GetValueAsync(null, context);

            using var repository = GitRepositoryFactory.GetRepository(repositoryUrl);

            var branchPatterns = m_BranchPatterns.Select(x => new Wildcard(x)).ToList();

            var matchingBranches = repository.Branches
              .Where(branchName => branchPatterns.Any(pattern => pattern.IsMatch(branchName)))
              .ToList();

            var outputs = new List<IDocument>();
            foreach (var branchName in matchingBranches)
            {
                var commitId = repository.GetHeadCommitId(branchName);
                var rootDir = repository.GetRootDirectory(commitId);

                var branchOutputs = Globber.GetFiles(rootDir, m_FilePatterns).Select(file =>
                {
                    var metadata = new Dictionary<string, object>()
                    {
                        { GitKeys.GitRepositoryUrl, repositoryUrl },
                        { GitKeys.GitBranch, branchName },
                        { GitKeys.GitCommit, commitId.ToString() },
                        { GitKeys.GitRelativePath, file.FullName },
                    };

                    using var stream = file.GetContentStream();
                    return context.CreateDocument(metadata, stream);
                });

                outputs.AddRange(branchOutputs);
            }
            return outputs;
        }
    }
}
