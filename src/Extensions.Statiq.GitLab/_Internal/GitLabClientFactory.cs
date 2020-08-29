// Note: Adapted from https://github.com/ap0llo/changelog/blob/ca85f816078dedbed1bb93e66096796a44e89e0e/src/ChangeLog/Integrations/GitLab/GitLabClientFactory.cs

using GitLabApiClient;

namespace Grynwald.Extensions.Statiq.GitLab
{
    internal class GitLabClientFactory : IGitLabClientFactory
    {
        public IGitLabClient CreateClient(string hostName, string? accessToken)
        {
            accessToken ??= "";
            var client = new GitLabClient($"https://{hostName}/", accessToken);
            return client;
        }
    }
}
