using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocsTemplate.Modules;
using Grynwald.Extensions.Statiq.TestHelpers;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Grynwald.Extensions.Statiq.DocsTemplate.Test.Modules
{
    /// <summary>
    /// Tests for <see cref="UseBootstrapTables"/>
    /// </summary>
    public class UseBootstrapTablesTest : TestBase
    {
        [Test]
        public async Task Module_Adds_table_tag()
        {
            // ARRANGE
            var document = new TestDocument(@"<html>
                        <head>
                            <title>Irrelevant</title>
                        </head>
                        <body>
                            <table>
                            </table>
                        </body>
                    </html>");

            var sut = new UseBootstrapTables();

            // ACT
            var result = await ExecuteAsync(document, sut).SingleAsync();

            // ASSERT
            var html = await result.ParseAsHtmlAsync();

            html.QuerySelectorAll("table")
                .Should().ContainSingle()
                .Which.Should().BeAssignableTo<IHtmlTableElement>()
                .Which.ClassList
                    .Should().ContainSingle()
                    .And.Contain("table");
        }

        [Test]
        [TestCase(BootstrapTableOptions.Bordered, "table-bordered")]
        [TestCase(BootstrapTableOptions.Striped, "table-striped")]
        [TestCase(BootstrapTableOptions.Small, "table-sm")]
        public async Task Module_adds_specified_optional_classes(BootstrapTableOptions tableOptions, string expectedClass)
        {
            // ARRANGE
            var document = new TestDocument(@"<html>
                        <head>
                            <title>Irrelevant</title>
                        </head>
                        <body>
                            <table>
                            </table>
                        </body>
                    </html>");

            var sut = new UseBootstrapTables().WithTableOptions(tableOptions);

            // ACT
            var result = await ExecuteAsync(document, sut).SingleAsync();

            // ASSERT
            var html = await result.ParseAsHtmlAsync();

            html.QuerySelectorAll("table")
              .Should().ContainSingle()
              .Which.Should().BeAssignableTo<IHtmlTableElement>()
              .Which.ClassList
                  .Should().HaveCount(2)
                  .And.Contain(new[] { "table", expectedClass });
        }
    }
}
