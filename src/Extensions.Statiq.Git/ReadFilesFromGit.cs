using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grynwald.Extensions.Statiq.Git.Internal;
using Grynwald.Utilities;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.Git
{
    //TODO: Add support for file patterns
    //TODO: Remove hardcoded "master" branch, use repository default branch
    public class ReadFilesFromGit : Module
    {
        private readonly Config<string> m_RepositoryUrl;
        private IReadOnlyList<string> m_BranchPatterns = new[] { "master" };


        public ReadFilesFromGit(string repositoryUrl) : this(Config.FromValue(repositoryUrl))
        { }

        public ReadFilesFromGit(Config<string> repositoryUrl)
        {
            m_RepositoryUrl = repositoryUrl ?? throw new ArgumentNullException(nameof(repositoryUrl));
        }


        public ReadFilesFromGit WithBranchPatterns(params string[] branchPatterns)
        {
            m_BranchPatterns = branchPatterns.ToArray();
            return this;
        }


        protected async override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
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
                var branchOutputs = repository.GetFiles(branchName).Select(file =>
                {
                    var metadata = new Dictionary<string, object>()
                    {
                        { GitKeys.GitRepositoryUrl, repositoryUrl },
                        { GitKeys.GitBranch, branchName },
                        { GitKeys.GitCommit, file.Commit },
                        { GitKeys.GitRelativePath, file.Path },
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
