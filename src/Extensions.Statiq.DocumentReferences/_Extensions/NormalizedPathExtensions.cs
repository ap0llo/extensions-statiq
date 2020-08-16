using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    internal static class NormalizedPathExtensions
    {
        public static NormalizedPath ToAbsolutePath(this NormalizedPath path, NormalizedPath absoluteTo)
        {
            return path.IsAbsolute ? path : absoluteTo.Combine(path);
        }
    }
}
