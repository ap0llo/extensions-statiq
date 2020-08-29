// Note: Adapted from https://github.com/ap0llo/changelog/blob/ca85f816078dedbed1bb93e66096796a44e89e0e/src/ChangeLog/Integrations/GitHub/GitHubClientFactory.cs

using System;
using Octokit;

namespace Grynwald.Extensions.Statiq.GitHub
{
    internal class GitHubClientFactory : IGitHubClientFactory
    {
        public IGitHubClient CreateClient(string hostName, string? accessToken)
        {
            var client = new GitHubClient(new ProductHeaderValue("extensions-statiq-github"), new Uri($"https://{hostName}/"));

            if (!String.IsNullOrEmpty(accessToken))
            {
                client.Credentials = new Credentials(accessToken);
            }

            return client;
        }
    }
}
