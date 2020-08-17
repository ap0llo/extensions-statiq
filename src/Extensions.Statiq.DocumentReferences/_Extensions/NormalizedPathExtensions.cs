using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    internal static class NormalizedPathExtensions
    {
        //TODO: Introduce a "common" project for extension method duplicated across projects
        internal static NormalizedPath ToAbsolutePath(this NormalizedPath path, NormalizedPath absoluteTo)
        {
            return path.IsAbsolute ? path : absoluteTo.Combine(path);
        }
    }
}
