using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using Grynwald.Extensions.Statiq.DocumentReferences;
using Grynwald.Extensions.Statiq.Git;
using Statiq.Common;
using Statiq.Core;
using Statiq.Markdown;
using Statiq.Yaml;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Demo
{
    public class Content : Pipeline
    {
        public Content()
        {
            InputModules = new ModuleList()
            {
                // Read files from git
                new ReadFilesFromGit(Config.FromSetting("GitRemoteUrl"))
                    .WithBranchNames("master")
                    .WithTagNames("v*")
                    .WithFileNames("docs/**/*.md", "docs/**/docs.yml"),

                // Parse docs.yml and then remote it from the documents
                new ReadDirectoryMetadataFromInputFiles("docs.yml", new ParseYaml()),
                new FilterDocuments(Config.FromDocument(d => d.GetGitRelativePath().Name != "docs.yml")),

                // Parse document front matter
                new ExtractFrontMatter(new ParseYaml()),

                // For documents read from tags, infer version from tag name
                new ExecuteIf(
                    Config.FromDocument(d => d.GetGitTag() != null),
                    new SetMetadata("version", Config.FromDocument(d => d.GetGitTag().TrimStart('v')))
                ),
                
                // Assign document reference metadata
                new SetDocumentReferenceMetadata(
                    Config.FromDocument(document => document.GetString("documentName") ?? document.GetGitRelativePath().ToString()),
                    Config.FromDocument(document => document.GetString("version"))
                ),
               
                // add metadata about other version of a document to the metadata
                new GatherVersions()
            };

            ProcessModules = new ModuleList()
            {
                new RenderMarkdown().UseExtensions(),
                new InferTitle(),
                new UseBootstrapTables().WithTableOptions(BootstrapTableOptions.Small| BootstrapTableOptions.Bordered),
                new LoadToc(),
                new RenderUsingDocsTemplateTheme(),
                new ResolveDocumentReferences().WithResolutionMode(LinkResolutionMode.Source),
                new MarkNavbarItemsAsActive().WithLinkMode(LinkMode.Source)
            };

            OutputModules = new ModuleList()
            {
                new SetDestination(Config.FromDocument(document =>
                {
                    return document.GetGitRelativePath()
                        .GetPathRelativeTo("docs")
                        .Prepend($"v{document.GetDocumentVersion()}")
                        .ChangeExtension(".html");
                })),
                new ResolveThemeLinks(),
                new ConvertSourceLinksToDestinationLinks(),
                new WriteFiles()
            };
        }
    }
}
