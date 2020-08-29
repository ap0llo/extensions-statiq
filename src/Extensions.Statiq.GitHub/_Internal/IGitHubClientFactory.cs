// Note: Adapted from https://github.com/ap0llo/changelog/blob/ca85f816078dedbed1bb93e66096796a44e89e0e/src/ChangeLog/Integrations/GitHub/IGitHubClientFactory.cs

using Octokit;

namespace Grynwald.Extensions.Statiq.GitHub
{
    internal interface IGitHubClientFactory
    {
        IGitHubClient CreateClient(string hostName, string? accessToken);
    }
}

