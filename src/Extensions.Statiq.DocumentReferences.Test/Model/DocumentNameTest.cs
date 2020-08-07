using System;
using System.Collections.Generic;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test.Model
{
    /// <summary>
    /// Tests for <see cref="DocumentName"/>.
    /// </summary>
    public class DocumentNameTest
    {
        public static IEnumerable<object?[]> InvalidNames()
        {
            // Value must not be null or whitespace

            yield return new object?[] { null };
            yield return new object?[] { "" };
            yield return new object?[] { "  " };
            yield return new object?[] { "\t" };

            // Value must not be "this"
            yield return new object?[] { "this" };
            yield return new object?[] { "THIS" };
            yield return new object?[] { "tHiS" };

            // Value must not contain leading or trailing whitespace
            yield return new object?[] { " id" };
            yield return new object?[] { "id " };
            yield return new object?[] { " id " };
            yield return new object?[] { "\tid " };
            yield return new object?[] { "\tid\t" };
        }

        [TestCaseSource(nameof(InvalidNames))]
        public void Constructor_throws_ArgumentException_for_invalid_values(string value)
        {
            Action act = () => new DocumentName(value);
            act.Should().Throw<ArgumentException>();
        }

        [TestCaseSource(nameof(InvalidNames))]
        public void TryCreate_returns_false_for_invalid_values(string value)
        {
            var success = DocumentName.TryCreate(value, out var name);

            success.Should().BeFalse();
            name.Should().BeNull();
        }

        [TestCase("@")]
        [TestCase("#")]
        public void Constructor_throws_ArgumentException_if_value_contains_invalid_characters(string reserved)
        {
            var actions = new Action[]
            {
                () => new DocumentName($"prefix{reserved}suffix"),
                () => new DocumentName($"prefix{reserved}"),
                () => new DocumentName($"{reserved}suffix"),
                () => new DocumentName(reserved)
            };

            foreach (var act in actions)
            {
                act.Should().Throw<ArgumentException>();
            }
        }

        [TestCase("@")]
        [TestCase("#")]
        public void TryCreate_returns_false_if_value_contains_invalid_characters(string reserved)
        {
            var success = DocumentName.TryCreate($"prefix{reserved}suffix", out var name);
            success.Should().BeFalse();
            name.Should().BeNull();

            success = DocumentName.TryCreate($"prefix{reserved}", out name);
            success.Should().BeFalse();
            name.Should().BeNull();

            success = DocumentName.TryCreate($"{reserved}suffix", out name);
            success.Should().BeFalse();
            name.Should().BeNull();

            success = DocumentName.TryCreate(reserved, out name);
            success.Should().BeFalse();
            name.Should().BeNull();
        }

        [TestCase("some-id")]
        public void TryCreate_returns_true_if_value_is_valid_id(string value)
        {
            var success = DocumentName.TryCreate(value, out var name);
            success.Should().BeTrue();
            name.Should().NotBeNull();
        }

        [Test]
        public void DocsId_instances_can_implicitly_be_converted_to_string()
        {
            var name = new DocumentName("value");
            string value = name;

            value.Should().Be("value");
        }

        [Test]
        public void ToString_returns_value()
        {
            var name = new DocumentName("value");
            var value = name.ToString();

            value.Should().Be("value");
        }

        [TestCase("value", "value")]
        [TestCase("value", "VALUE")]
        [TestCase("vaLUe", "value")]
        public void Comparision_of_two_equal_instances_yield_expected_result(string left, string right)
        {
            var leftName = new DocumentName(left);
            var rightName = new DocumentName(right);

            // instances must be equal to themselves
            Assert.AreEqual(leftName, leftName);
            Assert.AreEqual(rightName, rightName);

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(leftName == leftName);
            Assert.False(leftName != leftName);
            Assert.True(rightName == rightName);
            Assert.False(rightName != rightName);
#pragma warning restore CS1718 // Comparison made to same variable

            // hash code must be equal
            Assert.AreEqual(leftName.GetHashCode(), rightName.GetHashCode());

            Assert.AreEqual(leftName, rightName);
            Assert.AreEqual(rightName, leftName);
            Assert.True(leftName.Equals(rightName));
            Assert.True(leftName.Equals((object)rightName));
            Assert.True(rightName.Equals(leftName));
            Assert.True(rightName.Equals((object)leftName));

            var isEqual1 = leftName == rightName;
            var isEqual2 = rightName == leftName;
            var isNotEqual1 = leftName != rightName;
            var isNotEqual2 = rightName != leftName!;

            Assert.True(isEqual1);
            Assert.True(isEqual2);
            Assert.False(isNotEqual1);
            Assert.False(isNotEqual2);
        }

        [Test]
        public void DocumentId_instances_can_be_compared_to_null()
        {
            (new DocumentName("value") == null).Should().BeFalse();
            (null == new DocumentName("value")).Should().BeFalse();

            (new DocumentName("value") != null).Should().BeTrue();
            (null != new DocumentName("value")).Should().BeTrue();

            (null == (DocumentName?)null).Should().BeTrue();
            (null != (DocumentName?)null).Should().BeFalse();
        }
    }
}
