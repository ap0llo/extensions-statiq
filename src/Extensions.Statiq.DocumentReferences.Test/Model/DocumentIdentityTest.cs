using System;
using System.Collections.Generic;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NuGet.Versioning;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test.Model
{
    /// <summary>
    /// Tests for <see cref="DocumentIdentity"/>
    /// </summary>
    public class DocumentIdentityTest
    {
        [Test]
        public void Name_must_not_be_null()
        {
            Action act = () => new DocumentIdentity(null!, NuGetVersion.Parse("1.0"));
            act.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Version_must_not_be_null()
        {
            Action act = () => new DocumentIdentity(new DocumentName("value"), null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [TestCase("name1", "1.0", "name1", "1.0")]
        [TestCase("name1", "1.0", "name1", "1.0.0")]
        [TestCase("name1", "1.0", "NAME1", "1.0")]
        public void Comparision_of_two_equal_instances_yield_expected_result(string name1, string version1, string name2, string version2)
        {
            // ARRANGE
            var identity1 = new DocumentIdentity(new DocumentName(name1), NuGetVersion.Parse(version1));
            var identity2 = new DocumentIdentity(new DocumentName(name2), NuGetVersion.Parse(version2));

            // ACT / ASSERT

            Assert.AreEqual(identity1.GetHashCode(), identity2.GetHashCode());

            Assert.AreEqual(identity1, identity2);
            Assert.AreEqual(identity2, identity1);
            Assert.AreEqual((object)identity1, (object)identity2);
            Assert.AreEqual((object)identity2, (object)identity1);

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(identity1 == identity1);
            Assert.True(identity2 == identity2);
            Assert.False(identity1 != identity1);
            Assert.False(identity2 != identity2);
#pragma warning restore CS1718 // Comparison made to same variable
            Assert.True(identity1 == identity2);
            Assert.True(identity2 == identity1);

            Assert.False(identity1 != identity2);
            Assert.False(identity2 != identity1);
        }

        [Test]
        public void DocumentIdentity_instances_can_be_compared_to_null()
        {
            Assert.False(DocumentIdentity.Parse("name@1.0") == null);
            Assert.False(null == DocumentIdentity.Parse("name@1.0"));

            Assert.True(DocumentIdentity.Parse("name@1.0") != null);
            Assert.True(null != DocumentIdentity.Parse("name@1.0"));

            Assert.True((DocumentIdentity?)null == (DocumentIdentity?)null);
            Assert.False((DocumentIdentity?)null != (DocumentIdentity?)null);
        }

        [TestCase("name1", "1.0", "name1@1.0")]
        [TestCase("name1", "1.0.0", "name1@1.0.0")]
        [TestCase("name1", "1.0-alpha", "name1@1.0-alpha")]
        public void ToString_returns_expected_value(string name, string version, string expectedValue)
        {
            var identity = new DocumentIdentity(new DocumentName(name), NuGetVersion.Parse(version));
            identity.ToString().Should().Be(expectedValue);
        }

        public static IEnumerable<object[]> ValidInputs()
        {
            yield return new object[] { "name@1.0", "name", "1.0.0" };
            yield return new object[] { "name@1.0.0", "name", "1.0" };
            yield return new object[] { "name@1.0-alpha", "name", "1.0-alpha" };
        }

        [TestCaseSource(nameof(ValidInputs))]
        public void TryParse_returns_true_for_valid_inputs(string value, string name, string version)
        {
            // ARRANGE
            var expected = new DocumentIdentity(new DocumentName(name), NuGetVersion.Parse(version));

            // ACT 
            var success = DocumentIdentity.TryParse(value, out var parsed);

            // ASSERT
            success.Should().BeTrue();
            parsed.Should().Be(expected);
        }

        [TestCaseSource(nameof(ValidInputs))]
        public void Parse_returns_expected_identity_for_valid_inputs(string value, string name, string version)
        {
            // ARRANGE
            var expected = new DocumentIdentity(new DocumentName(name), NuGetVersion.Parse(version));

            // ACT 
            var actual = DocumentIdentity.Parse(value);

            // ASSERT
            actual.Should().Be(expected);
        }

        public static IEnumerable<object?[]> InvalidInputs()
        {
            // Value must not be null or whitespace
            yield return new object?[] { null };
            yield return new object?[] { "" };
            yield return new object?[] { " " };
            yield return new object?[] { "\t" };
            // value must be in format NAME@VERSION
            yield return new object?[] { "name" };
            yield return new object?[] { "1.0" };
            yield return new object?[] { "name@@1.0" };
            yield return new object?[] { "name@1.0@" };

            // Version must be a valid version
            yield return new object?[] { "name@not-a-version" };

            // Id must be a valid id
            yield return new object?[] { " name@1.0" };
            yield return new object?[] { "name @1.0" };
            yield return new object?[] { "this@1.0" };
        }

        [TestCaseSource(nameof(InvalidInputs))]
        public void TryParse_returns_false_for_invalid_inputs(string value)
        {
            // ARRANGE

            // ACT 
            var success = DocumentIdentity.TryParse(value, out var parsed);

            // ASSERT
            success.Should().BeFalse();
            parsed.Should().BeNull();
        }

        [TestCaseSource(nameof(InvalidInputs))]
        public void Parse_throws_ArgumentException_for_invalid_inputs(string value)
        {
            Action act = () => DocumentIdentity.Parse(value);
            act.Should().ThrowExactly<ArgumentException>();
        }

        [Test]
        public void WithName_returns_a_new_instance_with_a_different_id()
        {
            // ARRANGE
            var initialIdentity = new DocumentIdentity(new DocumentName("name1"), NuGetVersion.Parse("1.0"));
            var expectedNewIdentity = new DocumentIdentity(new DocumentName("name2"), NuGetVersion.Parse("1.0"));

            // ACT 
            var actualNewIdentity = initialIdentity.WithName(new DocumentName("name2"));

            // ASSERT
            actualNewIdentity
                .Should().NotBeSameAs(initialIdentity)
                .And.Be(expectedNewIdentity);
        }

        [Test]
        public void WithVersion_returns_a_new_instance_with_a_different_version()
        {
            // ARRANGE
            var initialIdentity = new DocumentIdentity(new DocumentName("name1"), NuGetVersion.Parse("1.0"));
            var expectedNewIdentity = new DocumentIdentity(new DocumentName("name1"), NuGetVersion.Parse("2.0"));

            // ACT 
            var actualNewIdentity = initialIdentity.WithVersion(NuGetVersion.Parse("2.0"));

            // ASSERT
            actualNewIdentity
                .Should().NotBeSameAs(initialIdentity)
                .And.Be(expectedNewIdentity);
        }
    }
}
