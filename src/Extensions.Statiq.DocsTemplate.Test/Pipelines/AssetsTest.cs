using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Pipelines;
using NUnit.Framework;
using Statiq.App;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Pipelines
{
    /// <summary>
    /// Tests for the <see cref="Assets"/> pipeline
    /// </summary>
    public class AssetsTest : IntegrationTestBase
    {
        private static readonly string[] EmptyThemeFiles =
        {
            "assets/js/site.js",
            "assets/css/site.css"
        };

        private static readonly string[] DefaultThemeFiles =
        {
            "assets/js/site.js",
            "assets/css/site.css",
            "assets/css/bootstrap/css/bootstrap.min.css",
            "assets/js/bootstrap/js/bootstrap.min.js",
            "assets/js/highlight.js/highlight.min.js",
            "assets/css/highlight.js/styles/default.min.css",
            "assets/js/jquery/jquery.min.js",
            "assets/js/popper.js/popper.min.js",
            "assets/js/mermaid/mermaid.min.js"
        };


        public static IEnumerable<object[]> ExpectedFileTestCases()
        {
            // When no theme is set, the files of the default themes should be copied
            yield return new object[] { null!, DefaultThemeFiles };

            // When theme is set to "Default", the files of the default theme should be copied            
            yield return new object[] { DocsTemplateThemeNames.Default, DefaultThemeFiles };

            // When theme is set to "Empty", the files of the empty themes should be copied
            yield return new object[] { DocsTemplateThemeNames.Empty, EmptyThemeFiles };
        }

        [TestCaseSource(nameof(ExpectedFileTestCases))]
        public async Task Pipelines_copies_expected_files_for_the_configured_theme_to_the_output(string themeName, string[] expectedFiles)
        {
            // ARRANGE
            var bootstrapper = Bootstrapper.Factory
                .CreateDefaultWithout(Array.Empty<string>(), DefaultFeatures.Pipelines)
                .ConfigureEngine(e =>
                {
                    e.FileSystem.OutputPath = m_OutputDirectory.FullName;
                    e.FileSystem.InputPaths.Insert(0, BootstrapperExtensions.BuiltInInputPath);
                })
                .AddPipeline<Assets>();

            if (themeName != null)
            {
                bootstrapper = bootstrapper.AddSetting(DocsTemplateKeys.DocsTemplateThemeName, themeName);
            }

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var actualFiles = Directory.GetFiles(m_OutputDirectory, "*.*", SearchOption.AllDirectories)
                .Select(x => new NormalizedPath(m_OutputDirectory).GetRelativePath(x).ToString())
                .ToArray();

            actualFiles.Should().OnlyContain(file => expectedFiles.Contains(file));
            expectedFiles.Should().OnlyContain(expected => File.Exists(GetFullOutputPath(expected)));
        }

    }
}
