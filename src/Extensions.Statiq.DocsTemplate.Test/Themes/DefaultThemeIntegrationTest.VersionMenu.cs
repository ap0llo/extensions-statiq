using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NuGet.Versioning;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Themes
{
    public partial class DefaultThemeIntegrationTest
    {
        [Test]
        public async Task VersionMenu_is_not_included_if_input_document_has_no_references_to_other_versions()
        {
            // ARRANGE
            var metadata = new TestMetadata();
            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#versionMenu")
                .Should().BeEmpty();
        }

        [Test]
        public async Task VersionMenu_is_included_if_input_document_has_references_to_other_versions()
        {
            // ARRANGE
            var metadata = GetDocumentReferenceMetadata(version: "1.0");
            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#versionMenu")
                .Should().ContainSingle();
        }

        [Test]
        public async Task VersionMenu_is_not_included_if_ShowVersionMenu_is_false()
        {
            // ARRANGE
            var metadata = GetDocumentReferenceMetadata();
            metadata[DocsTemplateKeys.ShowVersionMenu] = false;

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#versionMenu")
                .Should().BeEmpty();
        }

        [Test]
        // No highlighting of versions
        [TestCase("1.0", "1.0", "1.0", VersionHighlightingMode.None, "Version: 1.0")]
        [TestCase("1.0", "2.0", "2.0", VersionHighlightingMode.None, "Version: 1.0")]
        // default behavior when neither HighlightLatestVersion nor HighlightLatestDocumentVersion are set: highlight latest version
        [TestCase("1.0", "1.0", "1.0", null, "Version: 1.0 (latest)")]
        [TestCase("1.0", "2.0", "2.0", null, "Version: 1.0")]
        // explicitly set to highlight latest version
        [TestCase("1.0", "1.0", "1.0", VersionHighlightingMode.LatestVersion, "Version: 1.0 (latest)")]
        [TestCase("1.0", "2.0", "2.0", VersionHighlightingMode.LatestVersion, "Version: 1.0")]
        // highlight latest document version
        [TestCase("1.0", "2.0", "1.0", VersionHighlightingMode.LatestDocumentVersion, "Version: 1.0 (latest)")]
        [TestCase("1.0", "3.0", "2.0", VersionHighlightingMode.LatestDocumentVersion, "Version: 1.0")]
        public async Task VersionMenu_shows_expected_text(string version, string latestVersion, string latestDocumentVersion, VersionHighlightingMode? highlightingMode, string expectedText)
        {
            // ARRANGE
            var metadata = GetDocumentReferenceMetadata(version: version, latestVersion: latestVersion, latestDocumentVersion: latestDocumentVersion);

            if (highlightingMode.HasValue)
                metadata[DocsTemplateKeys.VersionHighlightingMode] = highlightingMode.Value;

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelector("#versionMenu a.nav-link.dropdown-toggle")
                .TextContent.Trim()
                .Should().Be(expectedText);
        }

        [Test]
        public async Task VersionMenu_includes_links_to_other_versions()
        {
            // ARRANGE
            var metadata = GetDocumentReferenceMetadata(
                name: "documentName",
                version: "1.0",
                latestVersion: "2.0",
                allVersions: new[] { "1.0", "1.5", "2.0" }
            );
            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var html = GetHtmlOutput();

            var dropdownItems = html.QuerySelectorAll("#versionMenu .dropdown-menu .dropdown-item");
            dropdownItems.Should().HaveCount(3);

            dropdownItems.Select(x => x.GetAttribute("href"))
                .Should().Equal(new[] { "ref:documentName@2.0", "ref:documentName@1.5", "ref:documentName@1.0" });
        }

        [Test]
        // No highlighting of versions
        [TestCase("2.0", "2.0", VersionHighlightingMode.None, new string[] { "v2.0", "v1.5", "v1.0" })]
        [TestCase("2.0", "1.5", VersionHighlightingMode.None, new string[] { "v2.0", "v1.5", "v1.0" })]
        // default behavior when neither HighlightLatestVersion nor HighlightLatestDocumentVersion are set: highlight latest version
        [TestCase("2.0", "1.5", null, new string[] { "v2.0 (latest)", "v1.5", "v1.0" })]
        // highlight latest version
        [TestCase("2.0", "1.5", VersionHighlightingMode.LatestVersion, new string[] { "v2.0 (latest)", "v1.5", "v1.0" })]
        // highlight latest document version
        [TestCase("2.0", "1.5", VersionHighlightingMode.LatestDocumentVersion, new string[] { "v2.0", "v1.5 (latest)", "v1.0" })]
        public async Task VersionMenu_shows_expected_text_in_dropdown(string latestVersion, string latestDocumentVersion, VersionHighlightingMode? highlightingMode, string[] expectedTexts)
        {
            // ARRANGE
            var metadata = GetDocumentReferenceMetadata(
                name: "documentName",
                version: "1.0",
                latestVersion: latestVersion,
                latestDocumentVersion: latestDocumentVersion,
                allVersions: new[] { "1.0", "1.5", "2.0" }
            );

            if (highlightingMode.HasValue)
                metadata[DocsTemplateKeys.VersionHighlightingMode] = highlightingMode.Value;

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var html = GetHtmlOutput();

            var dropdownItems = html.QuerySelectorAll("#versionMenu .dropdown-menu .dropdown-item");
            dropdownItems.Should().HaveCount(3);

            dropdownItems.Select(x => x.TextContent).Should().Equal(expectedTexts);
        }

        [Test]
        public async Task VersionMenu_is_not_a_dropdown_menu_if_there_are_not_at_least_two_versions([Range(0, 1)] int versionCount)
        {
            // ARRANGE
            var metadata = GetDocumentReferenceMetadata(
                allVersions: Enumerable.Range(1, versionCount).Select(x => $"{x}.0")
            );

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var html = GetHtmlOutput();

            var dropdownItems = html.QuerySelectorAll("#versionMenu .dropdown-menu .dropdown-item");
            dropdownItems.Should().BeEmpty();
        }

        //TODO: current item should be active => handled by MarkNavbarItemsAsActive?


        private TestMetadata GetDocumentReferenceMetadata(
            string name = "name", string version = "1.0",
            string latestVersion = "1.0", string latestDocumentVersion = "1.0",
            IEnumerable<string>? allVersions = null)
        {
            allVersions ??= new[] { version, latestDocumentVersion, latestVersion }.Distinct().ToArray();

            var documentName = new DocumentName(name);
            var documentVersion = NuGetVersion.Parse(version);
            var documentIdentity = new DocumentIdentity(documentName, documentVersion);

            return new TestMetadata()
            {
                { DocumentReferenceKeys.DocumentName, documentName },
                { DocumentReferenceKeys.DocumentVersion, documentVersion },
                { DocumentReferenceKeys.DocumentIdentity, documentIdentity },
                { DocumentReferenceKeys.DocumentReference, new FullyQualifiedDocumentReference(documentIdentity) },
                { DocumentReferenceKeys.LatestDocumentVersion, NuGetVersion.Parse(latestDocumentVersion) },
                { DocumentReferenceKeys.LatestVersion, NuGetVersion.Parse(latestVersion) },
                {
                    DocumentReferenceKeys.AllDocumentVersions,
                    allVersions
                        .Select(v =>
                        {
                            var version = NuGetVersion.Parse(v);
                            var identity = new DocumentIdentity(documentName, version);
                            return new TestDocument(new TestMetadata()
                            {
                                { DocumentReferenceKeys.DocumentName, documentName },
                                { DocumentReferenceKeys.DocumentVersion, version },
                                { DocumentReferenceKeys.DocumentIdentity, identity },
                                { DocumentReferenceKeys.DocumentReference, new FullyQualifiedDocumentReference(identity) }
                            });
                        })
                        .Cast<IDocument>()
                        .ToArray()
                }
            };
        }

    }
}
