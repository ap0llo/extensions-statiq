using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public class GitFileInfo : FileInfoBase
    {
        private readonly GitDirectoryInfo m_Parent;

        public override string Name { get; }

        public override string FullName
        {
            get
            {
                var parentPath = m_Parent?.FullName;
                return String.IsNullOrEmpty(parentPath) ? Name : $"{parentPath}/{Name}";
            }
        }

        public override DirectoryInfoBase ParentDirectory => m_Parent;

        public GitFileInfo(string name, GitDirectoryInfo parent)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            if (name.Contains("\\") || name.Contains("/"))
                throw new ArgumentException("Name must not contain directory separators", nameof(name));

            Name = name;
            m_Parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }

    }

}
