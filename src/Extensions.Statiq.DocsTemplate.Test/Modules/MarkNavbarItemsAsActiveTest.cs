using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Modules
{
    /// <summary>
    /// Tests for <see cref="MarkNavbarItemsAsActive"/>
    /// </summary>
    public class MarkNavbarItemsAsActiveTest : TestBase
    {
        [Test]
        // Base tests cases: All values of LinkMode, simple source or destination
        [TestCase(null, "/source.html", "destination.html", "destination.html")]
        [TestCase(LinkMode.Destination, "/source.html", "destination.html", "destination.html")]
        [TestCase(LinkMode.Source, "/source.html", "Irrelevant", "source.html")]
        // Relative paths in href
        [TestCase(LinkMode.Destination, "/dir1/source.html", "dir1/destination.html", "../dir1/destination.html")]
        [TestCase(LinkMode.Source, "/dir1/dir2/source.html", "Irrelevant", "../dir2/source.html")]
        public async Task Execute_marks_items_as_active_if_they_link_to_the_current_documents_destination(LinkMode? linkMode, string source, string destination, string href)
        {
            // ARRANGE
            var input = new TestDocument(
                new NormalizedPath(source),
                new NormalizedPath(destination),
                $@"<html>
                    <body>
                        <nav class=""navbar"">
                            <ul class=""navbar-nav"">
                                <li class=""nav-item"">
                                    <a class=""nav-link"" href=""{href}"">Link Text</a>
                                </li>
                                <li class=""nav-item"">
                                    <a class=""nav-link"" href=""some-other-link"">Link Text</a>
                                </li>
                            </ul>
                        </nav>   
                    </body>
                </html>");

            var sut = new MarkNavbarItemsAsActive();
            if (linkMode.HasValue)
            {
                sut = sut.WithLinkMode(linkMode.Value);
            }

            // ACT
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            var html = await output.ParseAsHtmlAsync();
            var anchors = html.QuerySelectorAll("a");

            anchors.Should().HaveCount(2);

            {
                var anchor = anchors.First();
                anchor.GetAttribute("href")
                    .Should().Be("#");
                anchor.ClassList
                    .Should().HaveCount(2)
                    .And.Contain("active")
                    .And.Contain("nav-link");
            }
            {
                var anchor = anchors.Last();
                anchor.GetAttribute("href")
                    .Should().Be("some-other-link");
                anchor.ClassList
                    .Should().ContainSingle()
                    .And.Contain("nav-link");
            }
        }


        [Test]
        // Base tests cases: All values of LinkMode, simple source or destination
        [TestCase(null, "/source.html", "destination.html", "destination.html")]
        [TestCase(LinkMode.Destination, "/source.html", "destination.html", "destination.html")]
        [TestCase(LinkMode.Source, "/source.html", "Irrelevant", "source.html")]
        // Relative paths in href
        [TestCase(LinkMode.Destination, "/dir1/source.html", "dir1/destination.html", "../dir1/destination.html")]
        [TestCase(LinkMode.Source, "/dir1/dir2/source.html", "Irrelevant", "../dir2/source.html")]
        public async Task Execute_marks_items_in_dropdown_menus_as_active_if_they_link_to_the_current_documents_destination(LinkMode? linkMode, string source, string destination, string href)
        {
            // ARRANGE
            var input = new TestDocument(
                new NormalizedPath(source),
                new NormalizedPath(destination),
                $@"<html>
                    <body>
                        <nav class=""navbar"">
                            <ul class=""navbar-nav"">
                                <li class=""nav-item dropdown"">
                                    <a class=""nav-link dropdown-toggle"" href=""some-other-link"">Dropdown Text</a>
                                    <div class=""dropdown-menu"">
                                        <a class=""dropdown-item"" href=""some-other-link"">Link Text</a>
                                        <a class=""dropdown-item"" href=""{href}"">Link Text</a>
                                    </div>
                                </li>
                            </ul>
                        </nav>   
                    </body>
                </html>");

            var sut = new MarkNavbarItemsAsActive();
            if (linkMode.HasValue)
            {
                sut = sut.WithLinkMode(linkMode.Value);
            }

            // ACT
            var output = await ExecuteAsync(input, sut).SingleAsync();

            // ASSERT
            var html = await output.ParseAsHtmlAsync();

            // both the dropdown-item and the dropdown-toggle must be marked as active
            var dropdownToggle = html.QuerySelectorAll(".dropdown-toggle");
            dropdownToggle.Should().ContainSingle();
            dropdownToggle.Single().ClassList
                .Should().HaveCount(3)
                .And.Contain("nav-link")
                .And.Contain("dropdown-toggle")
                .And.Contain("active");

            var dropdownItem = html.QuerySelectorAll("a.dropdown-item");
            dropdownItem.Should().HaveCount(2);
            dropdownItem.Last().GetAttribute("href").Should().Be("#");
            dropdownItem.Last().ClassList
                .Should().HaveCount(2)
                .And.Contain("dropdown-item")
                .And.Contain("active");
        }
    }
}
