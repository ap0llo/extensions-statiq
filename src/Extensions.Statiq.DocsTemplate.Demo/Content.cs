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
                new ReadFilesFromGit(Config.FromSetting("GitRemoteUrl"))
                    .WithBranchNames("master", "release/*")
                    .WithFileNames("docs/**/*.md", "docs/**/docs.yml"),

                new ReadDirectoryMetadataFromInputFiles("docs.yml", new ParseYaml()),
                new FilterDocuments(Config.FromDocument(d => d.GetGitRelativePath().Name != "docs.yml")),

                new ExtractFrontMatter(new ParseYaml()),
                new SetDocumentReferenceMetadata(
                    Config.FromDocument(document => document.GetString("documentName") ?? document.GetGitRelativePath().ToString()),
                    Config.FromDocument(document => document.GetString("version"))
                ),
                new GatherVersions(),
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
