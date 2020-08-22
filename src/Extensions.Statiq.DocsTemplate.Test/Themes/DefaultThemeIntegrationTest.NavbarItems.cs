using System;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using FluentAssertions;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Themes
{
    public partial class DefaultThemeIntegrationTest : ThemeIntegrationTestBase
    {
        [Test]
        public async Task NavbarItems_are_not_included_if_input_document_has_no_NavbarItems_metadata()
        {
            // ARRANGE
            var metadata = new TestMetadata();

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#navbarItems")
                .Should().BeEmpty();
        }

        [Test]
        public async Task NavbarItems_are_not_included_if_NavbarItems_is_empty()
        {
            // ARRANGE
            var metadata = GetMetadataWithNavbarItems(Array.Empty<IMetadata>());

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "<p>Some Content</p>", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#navbarItems")
                .Should().BeEmpty();
        }

        [Test]
        public async Task NavbarItems_are_not_included_if_ShowNavbarItems_is_false()
        {
            // ARRANGE
            var metadata = GetMetadataWithNavbarItems(
                GetNavbarLinkMetadata("Link 1", "http://example.com"),
                GetNavbarLinkMetadata("Link 2", "link-uri")
            );
            metadata[DocsTemplateKeys.ShowNavbarItems] = false;

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            GetHtmlOutput()
                .QuerySelectorAll("#navbarItems")
                .Should().BeEmpty();
        }

        [Test]
        public async Task NavbarItems_includes_links_from_metadata()
        {
            // ARRANGE
            var metadata = GetMetadataWithNavbarItems(
                GetNavbarLinkMetadata("Link 1", "http://example.com"),
                GetNavbarLinkMetadata("Link 2", "link-uri")
            );

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var html = GetHtmlOutput();
            html
                .QuerySelectorAll("#navbarItems")
                .Should().ContainSingle();

            var navItems = html.QuerySelectorAll("#navbarItems .nav-item a");

            navItems.Should().HaveCount(2);

            {
                var item1 = navItems.First();
                var anchor = item1.Should().BeAssignableTo<IHtmlAnchorElement>().Which;
                anchor.ClassList.Should().Contain("nav-link");
                anchor.InnerHtml.Should().Be("Link 1");
                anchor.GetAttribute("href").Should().Be("http://example.com");
            }
            {
                var item2 = navItems.Last();
                var anchor = item2.Should().BeAssignableTo<IHtmlAnchorElement>().Which;
                anchor.ClassList.Should().Contain("nav-link");
                anchor.InnerHtml.Should().Be("Link 2");
                anchor.GetAttribute("href").Should().Be("link-uri");
            }
        }

        [Test]
        public async Task NavbarItems_includes_links_from_hierarchical_metadata()
        {
            // ARRANGE
            var childItems = GetMetadataWithNavbarItems(
                GetNavbarLinkMetadata("Link 1", "http://example.com"),
                GetNavbarLinkMetadata("Link 2", "link-uri")
            );
            childItems[DocsTemplateKeys.Title] = "Links";

            var metadata = GetMetadataWithNavbarItems(childItems);

            var bootstrapper = CreateBootstrapper(DocsTemplateThemeNames.Default, "", metadata);

            // ACT
            var result = await bootstrapper.RunTestAsync();

            // ASSERT
            result.ExitCode.Should().Be(0);

            var html = GetHtmlOutput();

            html.QuerySelectorAll("#navbarItems")
                .Should().ContainSingle();

            html.QuerySelectorAll("#navbarItems .nav-item")
                .Should().ContainSingle()
                .Which.ClassList.Should().Contain("dropdown");

            html.QuerySelectorAll("#navbarItems .nav-item a.nav-link")
                .Should().ContainSingle()
                .Which.InnerHtml.Trim().Should().Be("Links");

            var links = html.QuerySelectorAll("#navbarItems .nav-item div.dropdown-menu a");

            {
                var link = links.First();
                var anchor = link.Should().BeAssignableTo<IHtmlAnchorElement>().Which;
                anchor.ClassList.Should().Contain("dropdown-item");
                anchor.InnerHtml.Should().Be("Link 1");
                anchor.GetAttribute("href").Should().Be("http://example.com");
            }
            {
                var link = links.Last();
                var anchor = link.Should().BeAssignableTo<IHtmlAnchorElement>().Which;
                anchor.ClassList.Should().Contain("dropdown-item");
                anchor.InnerHtml.Should().Be("Link 2");
                anchor.GetAttribute("href").Should().Be("link-uri");
            }
        }


        private TestMetadata GetMetadataWithNavbarItems(params IMetadata[] navbarItems)
        {
            return new TestMetadata()
            {
                { DocsTemplateKeys.NavbarItems, navbarItems }
            };
        }

        private TestMetadata GetNavbarLinkMetadata(string title, string link)
        {
            return new TestMetadata()
            {
                { DocsTemplateKeys.Title, title },
                { DocsTemplateKeys.Link, link }
            };
        }
    }
}
