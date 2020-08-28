using LibGit2Sharp;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public static class GitObjectExtensions
    {
        public static GitId GetGitId(this GitObject gitObject)
        {
            var sha = ((IBelongToARepository)gitObject).Repository.ObjectDatabase.ShortenObjectId(gitObject);
            return new GitId(sha);
        }
    }
}
