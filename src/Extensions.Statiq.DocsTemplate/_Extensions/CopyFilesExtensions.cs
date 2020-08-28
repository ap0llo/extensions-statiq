using System;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Core;

namespace Grynwald.Extensions.Statiq.DocsTemplate
{
    internal static class CopyFilesExtensions
    {
        internal static CopyFiles To(this CopyFiles copyFiles, Func<NormalizedPath, NormalizedPath> destinationPath)
        {
            return copyFiles.To((IFile file) =>
            {
                var relativeInputPath = file.Path.GetRelativeInputPath();
                var result = destinationPath(relativeInputPath);
                return Task.FromResult(result);
            });
        }
    }
}
