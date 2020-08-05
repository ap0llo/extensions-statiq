using System.Collections.Generic;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public static class GitDirectoryInfoExtensions
    {
        public static IEnumerable<GitFileInfo> EnumerateFilesRescursively(this GitDirectoryInfo directory)
        {
            foreach (var subDirectory in directory.Directories)
            {
                foreach (var file in subDirectory.EnumerateFilesRescursively())
                {
                    yield return file;
                }
            }
            foreach (var file in directory.Files)
            {
                yield return file;
            }
        }
    }
}
