using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test.Model
{
    /// <summary>
    /// Tests for <see cref="DocumentReferenceResolver{T}"/>
    /// </summary>
    public class DocumentReferenceResolverTest
    {
        [TestCase("name1@1.0", "name2@2.3", "ref:name2@2.3", "document2")]
        [TestCase("name1@1.0", "name2@1.0", "ref:name2", "document2")]
        [TestCase("name@1.0", "name@2.0", "ref:this@2.0", "document2")]
        [TestCase("name1@1.0", "name2@2.0", "ref:this", "document1")]
        public void TryResolveDocument_returns_expected_document(string id1, string id2, string reference, string expectedResult)
        {
            // ARRANGE      

            var sut = new DocumentReferenceResolver<string>();
            sut.Add(DocumentIdentity.Parse(id1), "document1");
            sut.Add(DocumentIdentity.Parse(id2), "document2");

            // ACT
            var actualResult = sut.TryResolveDocument(DocumentReference.Parse(reference), "document1");

            // ASSERT
            actualResult
                .Should().NotBeNull()
                .And.Be(expectedResult);
        }

        [TestCase("ref:this@3.0")]
        [TestCase("ref:some-other-name")]
        [TestCase("ref:name@0.1")]
        public void TryResolveDocument_returns_null_if_document_cannot_be_resolved(string reference)
        {
            // ARRANGE      

            var sut = new DocumentReferenceResolver<string>();
            sut.Add(DocumentIdentity.Parse("name@1.0"), "document1");
            sut.Add(DocumentIdentity.Parse("name@2.0"), "document2");

            // ACT
            var result = sut.TryResolveDocument(DocumentReference.Parse(reference), "document1");

            // ASSERT
            result.Should().BeNull();
        }

        [TestCase("ref:name@0.1")]
        public void TryResolveDocument_can_resolved_fully_qualified_references_in_documents_without_identity(string reference)
        {
            // ARRANGE      

            var sut = new DocumentReferenceResolver<string>();
            sut.Add(DocumentIdentity.Parse("name@0.1"), "document");

            // ACT
            var result = sut.TryResolveDocument(DocumentReference.Parse(reference), "some-other-document");

            // ASSERT
            result
                .Should().NotBeNull()
                .And.Be("document");
        }
    }
}
