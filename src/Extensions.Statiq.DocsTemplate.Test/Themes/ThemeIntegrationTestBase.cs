﻿using System;
using System.IO;
using ApprovalTests;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using Statiq.App;
using Statiq.Common;
using Statiq.Core;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Themes
{
    public abstract class ThemeIntegrationTestBase : IntegrationTestBase
    {
        protected const string s_OutputFileName = "output.html";


        protected void Approve(string testCaseName)
        {
            var writer = new ApprovalTextWriter(LoadOutput(), "html");
            Approvals.Verify(writer, new ApprovalNamer(testCaseName), Approvals.GetReporter());
        }

        protected string LoadOutput()
        {
            var outputPath = Path.Combine(m_OutputDirectory, s_OutputFileName);

            File.Exists(outputPath).Should().BeTrue(because: "Output file should exist");

            var content = File.ReadAllText(outputPath);
            return content;
        }

        protected Bootstrapper CreateBootstrapper(string? themeName, string inputContent)
        {
            var bootstrapper = Bootstrapper.Factory
                .CreateDefaultWithout(Array.Empty<string>(), DefaultFeatures.Pipelines)
                .ConfigureEngine(e =>
                {
                    e.FileSystem.OutputPath = m_OutputDirectory.FullName;
                    e.FileSystem.InputPaths.Insert(0, BootstrapperExtensions.BuiltInInputPath);
                })
                .BuildPipeline("TestPipeline", builder =>
                {
                    builder
                        .WithInputModules(new CreateDocuments(inputContent))
                        .WithProcessModules(new RenderUsingDocsTemplateTheme())
                        .WithOutputModules(
                            new SetDestination(s_OutputFileName),
                            new ResolveThemeLinks(),
                            new WriteFiles()
                        );
                });

            if (themeName != null)
            {
                bootstrapper = bootstrapper.AddSetting(DocsTemplateKeys.DocsTemplateThemeName, themeName);
            }

            return bootstrapper;
        }
    }
}
