using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using Grynwald.Extensions.Statiq.TestHelpers;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test
{
    /// <summary>
    /// Tests for <see cref="ResolveDocumentReferences"/>
    /// </summary>
    public class ResolveDocumentReferencesTest : TestBase
    {
        [TestCase(null, null, "./destination.html", null, "./destination2.html", "destination2.html")]
        [TestCase(null, null, "./directory/destination.html", null, "./destination2.html", "../destination2.html")]
        [TestCase(null, null, "./destination.html", null, "./directory/destination2.html", "directory/destination2.html")]
        [TestCase(null, null, "./directory1/destination.html", null, "./directory2/destination2.html", "../directory2/destination2.html")]
        [TestCase(LinkResolutionMode.Destination, null, "./destination.html", null, "./destination2.html", "destination2.html")]
        [TestCase(LinkResolutionMode.Destination, null, "./directory/destination.html", null, "./destination2.html", "../destination2.html")]
        [TestCase(LinkResolutionMode.Destination, null, "./destination.html", null, "./directory/destination2.html", "directory/destination2.html")]
        [TestCase(LinkResolutionMode.Destination, null, "./directory1/destination.html", null, "./directory2/destination2.html", "../directory2/destination2.html")]
        [TestCase(LinkResolutionMode.Source, "C:/source1.html", null, "C:/source2.html", null, "source2.html")]
        [TestCase(LinkResolutionMode.Source, "C:/directory/source1.html", null, "C:/source2.html", null, "../source2.html")]
        [TestCase(LinkResolutionMode.Source, "C:/directory1/source1.html", null, "C:/directory2/source2.html", null, "../directory2/source2.html")]
        public async Task Execute_resolves_references_to_the_expected_path(LinkResolutionMode? linkResolutionMode, string source1, string destination1, string source2, string destination2, string expectedPath)
        {
            // ARRANGE      
            var input1 = new TestDocument(
                source1 ?? "/source1.html",
                destination1 ?? "./destination1.html",
                GetIdentityMetadata("name1@1.0"),
                GetHtml(@"<a href=""ref:name2@2.0"">Link</a>"));

            var input2 = new TestDocument(
                source2 ?? "/source2.html",
                destination2 ?? "./destination2.html",
                GetIdentityMetadata("name2@2.0"),
                GetHtml());

            var input = new[] { input1, input2 };

            var sut = new ResolveDocumentReferences();

            if (linkResolutionMode.HasValue)
            {
                sut = sut.WithResolutionMode(linkResolutionMode.Value);
            }

            // ACT 
            var output = await ExecuteAsync(input, sut);

            // ASSERT
            output.Should().HaveCount(2);
            var output1 = output.First();

            var html = await output1.ParseAsHtmlAsync();

            html.QuerySelectorAll("a")
                .Should().ContainSingle("there should be a single <a/> element")
                .Which.Should().BeAssignableTo<IHtmlAnchorElement>()
                .Which.GetAttribute("href")
                .Should().Be(expectedPath);
        }

        [Test]
        public async Task Execute_uses_the_configured_config_to_retrieve_a_documents_identity()
        {
            // ARRANGE      
            var input1 = new TestDocument(
                "C:/source1.html",
                "./destination1.html",
                new TestMetadata(),
                GetHtml(@"<a href=""ref:name2@2.0"">Link</a>"));

            var input2 = new TestDocument(
                "C:/source2.html",
                "./destination2.html",
                new TestMetadata(),
                GetHtml());

            var input = new[] { input1, input2 };

            var sut = new ResolveDocumentReferences()
                .WithDocumentIdentity(Config.FromDocument(
                    d => d.Source == "C:/source1.html"
                        ? DocumentIdentity.Parse("name1@1.0")
                        : DocumentIdentity.Parse("name2@2.0")
                ));

            // ACT 
            var output = await ExecuteAsync(input, sut);

            // ASSERT
            output.Should().HaveCount(2);
            var output1 = output.First();

            var html = await output1.ParseAsHtmlAsync();

            html.QuerySelectorAll("a")
                .Should().ContainSingle("there should be a single <a/> element")
                .Which.Should().BeAssignableTo<IHtmlAnchorElement>()
                .Which.GetAttribute("href")
                .Should().Be("destination2.html");
        }

        [Test]
        public async Task Execute_ignores_documents_without_identity()
        {
            // ARRANGE      
            var input1 = new TestDocument(
                "C:/source1.html",
                "./destination1.html",
                new TestMetadata(),
                GetHtml());

            var input2 = new TestDocument(
                "C:/source2.html",
                "./destination2.html",
                new TestMetadata(),
                GetHtml());

            var input = new[] { input1, input2 };

            var sut = new ResolveDocumentReferences();

            // ACT 
            var output = await ExecuteAsync(input, sut);

            // ASSERT
            output.Should().HaveCount(2);
            m_TestExecutionContext.LogMessages.Should().OnlyContain(m => m.LogLevel == LogLevel.Warning && m.FormattedMessage.Contains("Failed to determine document identity"));
        }

        [Test]
        public async Task Execute_throws_DuplicateDocumentIdentityException_if_document_identities_are_not_unique()
        {
            // ARRANGE      
            var input1 = new TestDocument(
                "C:/source1.html",
                "./destination1.html",
                GetIdentityMetadata("id", "1.0"),
                GetHtml());

            var input2 = new TestDocument(
                "C:/source2.html",
                "./destination2.html",
                GetIdentityMetadata("id", "1.0"),
                GetHtml());

            var input = new[] { input1, input2 };

            var sut = new ResolveDocumentReferences();

            // ACT 
            Func<Task> act = async () => await ExecuteAsync(input, sut);

            // ASSERT
            await act.Should().ThrowAsync<DuplicateDocumentIdentityException>();
        }

        [Test]
        public async Task Execute_logs_warning_if_a_reference_could_not_be_resolved()
        {
            // ARRANGE      
            var input1 = new TestDocument(
                "C:/source1.html",
                "./destination1.html",
                GetIdentityMetadata("id1", "1.0"),
                GetHtml(@"<a href=""ref:unknown-id"">Link</a>"));

            var input2 = new TestDocument(
                "C:/source2.html",
                "./destination2.html",
                GetIdentityMetadata("id2", "1.0"),
                GetHtml());

            var input = new[] { input1, input2 };

            var sut = new ResolveDocumentReferences();

            // ACT 
            _ = await ExecuteAsync(input, sut);

            // ASSERT
            m_TestExecutionContext.LogMessages
                .Should().Contain(m => m.LogLevel == LogLevel.Warning)
                .Which.FormattedMessage.Should().Contain("ref:unknown-id");
        }

        [Test]
        public void Default_ResolutionMode_is_Destination()
        {
            var sut = new ResolveDocumentReferences();
            sut.ResolutionMode.Should().Be(LinkResolutionMode.Destination);
        }

        [TestCase(LinkResolutionMode.Destination)]
        [TestCase(LinkResolutionMode.Source)]
        public void WithResolutionMode_allows_setting_the_resolution_mode(LinkResolutionMode mode)
        {
            var sut = new ResolveDocumentReferences().WithResolutionMode(mode);
            sut.ResolutionMode.Should().Be(mode);
        }


        private static string GetHtml(string body = "")
        {
            return $@"<html>
                        <head>
                        </head>
                        <body>
                            {body}
                        </body>
                    </html>";
        }

        private static TestMetadata GetIdentityMetadata(string identity)
        {
            return new TestMetadata()
            {
                { DocumentReferenceKeys.DocumentIdentity,  DocumentIdentity.Parse(identity) }
            };
        }

        private static TestMetadata GetIdentityMetadata(string name, string version)
        {
            return new TestMetadata()
            {
                { DocumentReferenceKeys.DocumentIdentity, new DocumentIdentity(new DocumentName(name), NuGetVersion.Parse(version)) }
            };
        }
    }
}
