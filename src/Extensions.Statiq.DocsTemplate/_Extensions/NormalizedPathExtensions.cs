using System.Collections.Generic;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocsTemplate
{
    internal static class NormalizedPathExtensions
    {
        internal static NormalizedPath RemovePrefixDirectories(this NormalizedPath path, int count)
        {
            var parents = new List<string>();

            var current = path.Parent;
            while (!current.IsNullOrEmpty)
            {
                parents.Add(current.Name);
                current = current.Parent;
            }

            // determine the path to remove
            var pathPrefix = new NormalizedPath("");
            for (var i = 1; i <= count; i++)
            {
                pathPrefix = pathPrefix.Combine(parents[^i]);
            }

            return pathPrefix.GetRelativePath(path);
        }

        internal static NormalizedPath Prepend(this NormalizedPath path, string prefix) => new NormalizedPath(prefix) / path;
    }
}
