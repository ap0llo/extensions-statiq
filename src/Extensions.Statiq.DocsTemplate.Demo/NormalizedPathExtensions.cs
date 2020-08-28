using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Demo
{
    //TODO: Introduce a "common" project for extension method duplicated across projects

    internal static class NormalizedPathExtensions
    {
        internal static NormalizedPath GetPathRelativeTo(this NormalizedPath path, NormalizedPath from) => from.GetRelativePath(path);

        internal static NormalizedPath Prepend(this NormalizedPath path, string prefix) => new NormalizedPath(prefix) / path;
    }
}
