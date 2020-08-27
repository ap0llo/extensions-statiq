using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Adds metadata to input documents by reading other input documents.
    /// </summary>
    /// <remarks>
    /// For each input document, the directory and all parent directories are searched for files with the specified file name.
    /// If a file is found, metadata is read from it using the specified module and all non-existing metadata items are added to the input document.
    /// <para>
    /// The module will not look in the local filesystem for metadata files but only look into other input files of the module.
    /// </para>
    /// </remarks>
    /// <example>
    /// Assuming the module's input files are <c>/directory/input.md</c>, <c>/directory/data.yml</c> and <c>/data.yml</c>.
    /// The following code will read both <c>data.yml</c> files and add the parsed data to <c>input.md</c>:
    /// <code language="cs">
    /// new ReadDirectoryMetadataFromInputFiles("data.yml", new ParseYaml())
    /// </code>
    /// Because <c>/directory/data.yml</c> is located "closer" to the input file, metadata from that file will overwrite metadata from <c>/data.yml</c>.
    /// </example>
    public class ReadDirectoryMetadataFromInputFiles : ParentModule
    {
        private readonly Config<string> m_FileName;


        public ReadDirectoryMetadataFromInputFiles(string fileName, IModule module) : this(Config.FromValue(fileName), module)
        { }

        public ReadDirectoryMetadataFromInputFiles(Config<string> fileName, IModule module) : base(module)
        {
            m_FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }


        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            var metadataDocuments = await GetMetadataDocumentsAsync(input, context);

            foreach (var document in metadataDocuments)
            {
                var metadataDocument = (await context.ExecuteModulesAsync(Children, document.Yield())).Single();

                var newMetadata = metadataDocument.Where(kvp => !input.ContainsKey(kvp.Key));

                if (newMetadata.Any())
                    input = input.Clone(newMetadata);
            }

            return input.Yield();
        }


        /// <summary>
        /// Gets metadata files for the specified input ordered from most to least specific
        /// </summary>
        private async Task<IEnumerable<IDocument>> GetMetadataDocumentsAsync(IDocument document, IExecutionContext context)
        {
            var fileName = await m_FileName.GetValueAsync(document, context);

            var metadataDocuments = new List<IDocument>();

            var currentPath = document.Source.Parent;
            while (!currentPath.IsNullOrEmpty)
            {
                var metadataPath = currentPath.Combine(fileName);

                var metadataDocument = context.Inputs.SingleOrDefault(input => input.Source == metadataPath);
                if (metadataDocument != null)
                {
                    metadataDocuments.Add(metadataDocument);
                }

                currentPath = currentPath.Parent;
            }

            return metadataDocuments;
        }
    }
}
