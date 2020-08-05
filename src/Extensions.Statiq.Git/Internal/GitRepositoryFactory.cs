using System;
using System.Collections.Generic;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public static class GitRepositoryFactory
    {

        public static IGitRepository GetRepository(string repositoryUrl)
        {
            //TODO: remoteUrl must not be null or whitespace

            var repositoryKind = RepositoryKind.Unknown;
            if (Uri.TryCreate(repositoryUrl, UriKind.Absolute, out var uri))
            {
                if (uri.IsFile)
                {
                    repositoryKind = RepositoryKind.Local;
                }
                else
                {
                    repositoryKind = uri.Scheme.ToLower() switch
                    {
                        "http" => RepositoryKind.Remote,
                        "https" => RepositoryKind.Remote,
                        _ => RepositoryKind.Unknown
                    };
                }
            }

            switch (repositoryKind)
            {
                case RepositoryKind.Local:
                    return new LocalGitRepository(repositoryUrl);

                case RepositoryKind.Remote:
                    return new RemoteGitRepository(repositoryUrl);

                default:
                    throw new ArgumentException($"Unsupported repository url '{repositoryUrl}'");
            }
        }
    }
}
