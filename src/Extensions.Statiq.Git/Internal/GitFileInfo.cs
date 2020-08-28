using System;
using System.IO;
using LibGit2Sharp;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public class GitFileInfo : FileInfoBase
    {
        private readonly GitDirectoryInfo m_Parent;
        private readonly Blob m_Blob;


        public GitId Commit => m_Parent.Commit;

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


        public GitFileInfo(string name, GitDirectoryInfo parent, Blob blob)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            if (name.Contains("\\") || name.Contains("/"))
                throw new ArgumentException("Name must not contain directory separators", nameof(name));

            Name = name;
            m_Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            m_Blob = blob ?? throw new ArgumentNullException(nameof(blob));
        }


        public Stream GetContentStream() => m_Blob.GetContentStream();

    }

}
