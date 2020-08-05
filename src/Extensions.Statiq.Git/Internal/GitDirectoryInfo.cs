using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Grynwald.Extensions.Statiq.Git.Internal
{
    public class GitDirectoryInfo : DirectoryInfoBase
    {
        private readonly GitDirectoryInfo? m_Parent;
        private readonly Tree m_Tree;
        private readonly Lazy<Dictionary<string, GitDirectoryInfo>> m_Directories;
        private readonly Lazy<Dictionary<string, GitFileInfo>> m_Files;


        public override string Name { get; }

        public override string FullName
        {
            get
            {
                var parentPath = m_Parent?.FullName;
                return String.IsNullOrEmpty(parentPath) ? Name : $"{parentPath}/{Name}";
            }
        }

        public override DirectoryInfoBase? ParentDirectory => m_Parent;


        public IEnumerable<GitDirectoryInfo> Directories => m_Directories.Value.Values;

        public IEnumerable<GitFileInfo> Files => m_Files.Value.Values;


        public GitDirectoryInfo(Tree tree)
        {
            Name = "";
            m_Parent = null;
            m_Tree = tree ?? throw new ArgumentNullException(nameof(tree));

            m_Directories = new Lazy<Dictionary<string, GitDirectoryInfo>>(LoadDirectories);
            m_Files = new Lazy<Dictionary<string, GitFileInfo>>(LoadFiles);
        }

        public GitDirectoryInfo(string name, GitDirectoryInfo parent, Tree tree)
        {
            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be null or whitespace", nameof(name));

            if (name.Contains("\\") || name.Contains("/"))
                throw new ArgumentException("Name must not contain directory separators", nameof(name));

            Name = name;
            m_Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            m_Tree = tree ?? throw new ArgumentNullException(nameof(tree));

            m_Directories = new Lazy<Dictionary<string, GitDirectoryInfo>>(LoadDirectories);
            m_Files = new Lazy<Dictionary<string, GitFileInfo>>(LoadFiles);
        }


        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos()
        {
            return Enumerable.Concat(
                Directories.Cast<FileSystemInfoBase>(),
                Files.Cast<FileSystemInfoBase>()
            );
        }

        public override DirectoryInfoBase GetDirectory(string path)
        {
            var (name, remainingPath) = SplitPath(path);

            if (m_Directories.Value.TryGetValue(name, out var dir))
            {
                if (String.IsNullOrEmpty(remainingPath))
                {
                    return dir;
                }
                else
                {
                    return dir.GetDirectory(remainingPath);
                }

            }

            throw new DirectoryNotFoundException();
        }

        public override FileInfoBase GetFile(string path)
        {
            var (name, remainingPath) = SplitPath(path);

            if (String.IsNullOrEmpty(remainingPath))
            {
                if (m_Files.Value.TryGetValue(name, out var file))
                {
                    return file;
                }
                else
                {

                }
            }
            else if (m_Directories.Value.TryGetValue(name, out var dir))
            {
                return dir.GetFile(remainingPath);
            }

            throw new FileNotFoundException();
        }


        private Dictionary<string, GitDirectoryInfo> LoadDirectories()
        {
            var directories = new Dictionary<string, GitDirectoryInfo>();
            foreach (var item in m_Tree)
            {
                if (item.Target is Tree subTree)
                {
                    var dir = new GitDirectoryInfo(item.Name, this, subTree);
                    directories.Add(item.Name, dir);
                }
            }

            return directories;
        }

        private Dictionary<string, GitFileInfo> LoadFiles()
        {
            var files = new Dictionary<string, GitFileInfo>();

            foreach (var item in m_Tree)
            {
                if (item.Target is Blob blob)
                {
                    var file = new GitFileInfo(item.Name, this, blob);
                    files.Add(item.Name, file);
                }
            }

            return files;
        }

        private (string name, string remainingPath) SplitPath(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be null or whitespace", nameof(path));

            if (Path.IsPathRooted(path))
                throw new ArgumentException("Path must not be rooted", nameof(path));

            path = NormalizePath(path);

            if (path.Contains("/"))
            {
                var name = path.Substring(0, path.IndexOf("/"));
                var remainingPath = path.Substring(path.IndexOf("/") + 1);

                return (name, remainingPath);
            }
            else
            {
                return (path, "");
            }
        }

        private string NormalizePath(string path)
        {
            path = path.Replace("\\", "/");
            while (path.Contains("//"))
            {
                path = path.Replace("//", "/");
            }
            return path;
        }
    }

}
