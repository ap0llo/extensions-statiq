using System;
using System.Collections.Generic;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NuGet.Versioning;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test.Model
{
    /// <summary>
    /// Tests for <see cref="DocumentReference"/> and its derived classes
    /// </summary>
    public class DocsReferenceTest
    {
        public static IEnumerable<object?[]> InvalidReferences()
        {
            // null or whitespace
            yield return new object?[] { null };
            yield return new object?[] { "" };
            yield return new object?[] { "  " };
            yield return new object?[] { "\t" };

            // missing "ref:" scheme
            yield return new object?[] { "name@1.0" };
            yield return new object?[] { "this@1.0" };
            yield return new object?[] { "this" };
            yield return new object?[] { "name" };

            // wrong scheme (must be "ref")
            yield return new object?[] { "scheme:name@1.0" };
            yield return new object?[] { "scheme:this@1.0" };
            yield return new object?[] { "scheme:this" };
            yield return new object?[] { "scheme:name" };

            // leading whitespace
            yield return new object?[] { " ref:name@1.0" };
            yield return new object?[] { " ref:this@1.0" };
            yield return new object?[] { " ref:this" };
            yield return new object?[] { " ref:name" };

            // trailing whitespace
            yield return new object?[] { "ref:name@1.0 " };
            yield return new object?[] { "ref:this@1.0 " };
            yield return new object?[] { "ref:this " };
            yield return new object?[] { "ref:name " };

            // leading whitespace in name
            yield return new object?[] { "ref: name@1.0" };
            yield return new object?[] { "ref: this@1.0" };
            yield return new object?[] { "ref: this" };
            yield return new object?[] { "ref: name" };

            // trailing whitespace in name
            yield return new object?[] { "ref:name @1.0" };
            yield return new object?[] { "ref:this @1.0" };

            // invalid version
            yield return new object?[] { "ref:name@not-a-version" };
            yield return new object?[] { "ref:this@not-a-version" };
        }

        [TestCaseSource(nameof(InvalidReferences))]
        public void Parse_throws_ArgumentException_for_invalid_input(string value)
        {
            Action act = () => DocumentReference.Parse(value);
            act.Should().ThrowExactly<ArgumentException>();
        }

        [TestCaseSource(nameof(InvalidReferences))]
        public void TryParse_returns_false_for_invalid_input(string value)
        {
            var success = DocumentReference.TryParse(value, out var parsed);

            success.Should().BeFalse();
            parsed.Should().BeNull();
        }


        [TestCase("ref:name@1.0", "name", "1.0")]
        [TestCase("REF:name@1.0", "name", "1.0")]
        public void Parse_can_parse_fully_qualified_docs_references(string reference, string name, string version)
        {
            // ARRANGE
            var expectedIdentity = new DocumentIdentity(new DocumentName(name), NuGetVersion.Parse(version));

            // ACT 
            var parsed = DocumentReference.Parse(reference);

            // ASSERT
            parsed
                .Should().NotBeNull()
                .And.BeOfType<FullyQualifiedDocumentReference>()
                .And.Match<FullyQualifiedDocumentReference>(fqr => fqr.Identity == expectedIdentity);
        }

        [TestCase("ref:name@1.0", "name", "1.0")]
        [TestCase("REF:name@1.0", "name", "1.0")]
        public void Parse_returns_true_for_fully_qualified_docs_references(string reference, string name, string version)
        {
            // ARRANGE
            var expectedIdentity = new DocumentIdentity(new DocumentName(name), NuGetVersion.Parse(version));

            // ACT 
            var success = DocumentReference.TryParse(reference, out var parsed);

            // ASSERT
            success
                .Should().BeTrue();

            parsed
                .Should().NotBeNull()
                .And.BeOfType<FullyQualifiedDocumentReference>()
                .And.Match<FullyQualifiedDocumentReference>(fqr => fqr.Identity == expectedIdentity);
        }

        [TestCase("ref:this@1.0", "1.0")]
        [TestCase("REF:THIS@2.0", "2.0")]
        [TestCase("REF:tHiS@3.4", "3.4")]
        public void Parse_can_parse_same_name_docs_references(string reference, string version)
        {
            // ARRANGE
            var expectedVersion = NuGetVersion.Parse(version);

            // ACT 
            var parsed = DocumentReference.Parse(reference);

            // ASSERT
            parsed
                .Should().NotBeNull()
                .And.BeOfType<SameNameDocumentReference>()
                .And.Match<SameNameDocumentReference>(r => r.Version == expectedVersion);
        }

        [TestCase("ref:this@1.0", "1.0")]
        [TestCase("REF:THIS@2.0", "2.0")]
        [TestCase("REF:tHiS@3.4", "3.4")]
        public void TryParse_returns_true_for_same_name_docs_references(string reference, string version)
        {
            // ARRANGE
            var expectedVersion = NuGetVersion.Parse(version);

            // ACT 
            var success = DocumentReference.TryParse(reference, out var parsed);

            // ASSERT
            success
                .Should().BeTrue();

            parsed
                .Should().NotBeNull()
                .And.BeOfType<SameNameDocumentReference>()
                .And.Match<SameNameDocumentReference>(r => r.Version == expectedVersion);
        }

        [TestCase("ref:this")]
        [TestCase("REF:THIS")]
        [TestCase("REF:tHiS")]
        public void Parse_can_parse_self_references(string reference)
        {
            // ARRANGE

            // ACT 
            var parsed = DocumentReference.Parse(reference);

            // ASSERT
            parsed
                .Should().NotBeNull()
                .And.BeOfType<SelfDocumentReference>();
        }

        [TestCase("ref:this")]
        [TestCase("REF:THIS")]
        [TestCase("REF:tHiS")]
        public void TryParse_returns_true_for_self_references(string reference)
        {
            // ARRANGE

            // ACT 
            var success = DocumentReference.TryParse(reference, out var parsed);

            // ASSERT
            success
                .Should().BeTrue();

            parsed
                .Should().NotBeNull()
                .And.BeOfType<SelfDocumentReference>();
        }

        [TestCase("ref:name", "name")]
        [TestCase("REF:NAME", "name")]
        public void Parse_can_parse_same_version_references(string reference, string name)
        {
            // ARRANGE
            var expectedName = new DocumentName(name);

            // ACT 
            var parsed = DocumentReference.Parse(reference);

            // ASSERT
            parsed
                .Should().NotBeNull()
                .And.BeOfType<SameVersionDocumentReference>()
                .Which.Name.Should().Be(expectedName);
        }

        [TestCase("ref:name", "name")]
        [TestCase("REF:NAME", "name")]
        public void TryParse_returns_true_for_same_version_references(string reference, string name)
        {
            // ARRANGE
            var expectedName = new DocumentName(name);

            // ACT 
            var success = DocumentReference.TryParse(reference, out var parsed);

            // ASSERT
            success
                .Should().BeTrue();

            parsed
                .Should().NotBeNull()
                .And.BeOfType<SameVersionDocumentReference>()
                .Which.Name.Should().Be(expectedName);
        }

        [TestCase("ref:name@1.0", "ref:name@1.0")]
        [TestCase("ref:this@1.0", "ref:this@1.0")]
        [TestCase("ref:THIS@1.2", "ref:this@1.2")]
        [TestCase("ref:this", "ref:this")]
        [TestCase("ref:THIS", "ref:this")]
        [TestCase("ref:ThIs", "ref:this")]
        [TestCase("ref:name", "ref:name")]
        [TestCase("ref:some-other-name", "ref:some-other-name")]
        public void ToString_returns_expected_value(string referenceString, string expected)
        {
            // ARRANGE
            var reference = DocumentReference.Parse(referenceString);

            // ACT / ASSERT
            reference.ToString().Should().Be(expected);
        }

        // Base test cases: Same value for left and right
        [TestCase("ref:name@2.3", "ref:name@2.3")]
        [TestCase("ref:this@2.3", "ref:this@2.3")]
        [TestCase("ref:this", "ref:this")]
        [TestCase("ref:some-name", "ref:some-name")]
        // Names with different casing
        [TestCase("ref:name@2.3", "ref:NAME@2.3")]
        [TestCase("ref:this@2.3", "ref:THIS@2.3")]
        [TestCase("ref:this", "ref:tHiS")]
        [TestCase("ref:some-name", "ref:SOME-NAME")]
        // different (but equal) versions
        [TestCase("ref:name@2.3", "ref:name@2.3.0")]
        [TestCase("ref:this@2.3", "ref:this@2.3.0")]
        public void Comparision_of_two_equal_instances_yield_expected_result(string left, string right)
        {
            var leftReference = DocumentReference.Parse(left);
            var rightReference = DocumentReference.Parse(right);

            // instances must be equal to themselves
            Assert.AreEqual(leftReference, leftReference);
            Assert.AreEqual(rightReference, rightReference);

            // hash code must be equal
            Assert.AreEqual(leftReference.GetHashCode(), rightReference.GetHashCode());

            Assert.AreEqual(leftReference, rightReference);
            Assert.AreEqual(rightReference, leftReference);
            Assert.True(leftReference.Equals(rightReference));
            Assert.True(leftReference.Equals((object)rightReference));
            Assert.True(rightReference.Equals(leftReference));
            Assert.True(rightReference.Equals((object)leftReference));


            var isEqual1 = leftReference == rightReference;
            var isEqual2 = rightReference == leftReference;
            var isNotEqual1 = leftReference != rightReference;
            var isNotEqual2 = rightReference != leftReference!;
#pragma warning disable CS1718 // Comparison made to same variable
            var isEqual3 = rightReference == rightReference;
            var isNotEqual3 = rightReference != rightReference!;
#pragma warning restore CS1718 // Comparison made to same variable

            Assert.True(isEqual1);
            Assert.True(isEqual2);
            Assert.True(isEqual3);
            Assert.False(isNotEqual1);
            Assert.False(isNotEqual2);
            Assert.False(isNotEqual3);
        }

        [Test]
        public void DocsReference_instances_can_be_compared_to_null()
        {
            (DocumentReference.Parse("ref:id") == null).Should().BeFalse();
            (null == DocumentReference.Parse("ref:id")).Should().BeFalse();

            (DocumentReference.Parse("ref:id") != null).Should().BeTrue();
            (null != DocumentReference.Parse("ref:id")).Should().BeTrue();

            ((DocumentReference?)null == (DocumentReference?)null).Should().BeTrue();
            ((DocumentReference?)null != (DocumentReference?)null).Should().BeFalse();
        }
    }
}
