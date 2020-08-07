using System;
using System.Collections.Generic;
using FluentAssertions;
using Grynwald.Extensions.Statiq.DocumentReferences.Model;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.DocumentReferences.Test.Model
{
    /// <summary>
    /// Tests for <see cref="DocsId"/>.
    /// </summary>
    public class DocsIdTest
    {
        public static IEnumerable<object?[]> InvalidIds()
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

        [TestCaseSource(nameof(InvalidIds))]
        public void Constructor_throws_ArgumentException_for_invalid_values(string value)
        {
            Action act = () => new DocsId(value);
            act.Should().Throw<ArgumentException>();
        }

        [TestCaseSource(nameof(InvalidIds))]
        public void TryCreate_returns_false_for_invalid_values(string value)
        {
            var success = DocsId.TryCreate(value, out var id);

            success.Should().BeFalse();
            id.Should().BeNull();
        }

        [TestCase("@")]
        [TestCase("#")]
        public void Constructor_throws_ArgumentException_if_value_contains_invalid_characters(string reserved)
        {
            var actions = new Action[]
            {
                () => new DocsId($"prefix{reserved}suffix"),
                () => new DocsId($"prefix{reserved}"),
                () => new DocsId($"{reserved}suffix"),
                () => new DocsId(reserved)
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
            var success = DocsId.TryCreate($"prefix{reserved}suffix", out var id);
            success.Should().BeFalse();
            id.Should().BeNull();

            success = DocsId.TryCreate($"prefix{reserved}", out id);
            success.Should().BeFalse();
            id.Should().BeNull();

            success = DocsId.TryCreate($"{reserved}suffix", out id);
            success.Should().BeFalse();
            id.Should().BeNull();

            success = DocsId.TryCreate(reserved, out id);
            success.Should().BeFalse();
            id.Should().BeNull();
        }

        [TestCase("some-id")]
        public void TryCreate_returns_true_if_value_is_valid_id(string value)
        {
            var success = DocsId.TryCreate(value, out var id);
            success.Should().BeTrue();
            id.Should().NotBeNull();
        }

        [Test]
        public void DocsId_instances_can_implicitly_be_converted_to_string()
        {
            var uid = new DocsId("value");
            string value = uid;

            value.Should().Be("value");
        }

        [Test]
        public void ToString_returns_value()
        {
            var id = new DocsId("value");
            var value = id.ToString();

            value.Should().Be("value");
        }

        [TestCase("value", "value")]
        [TestCase("value", "VALUE")]
        [TestCase("vaLUe", "value")]
        public void Comparision_of_two_equal_instances_yield_expected_result(string left, string right)
        {
            var leftId = new DocsId(left);
            var rightId = new DocsId(right);

            // instances must be equal to themselves
            Assert.AreEqual(leftId, leftId);
            Assert.AreEqual(rightId, rightId);

#pragma warning disable CS1718 // Comparison made to same variable
            Assert.True(leftId == leftId);
            Assert.False(leftId != leftId);
            Assert.True(rightId == rightId);
            Assert.False(rightId != rightId);
#pragma warning restore CS1718 // Comparison made to same variable

            // hash code must be equal
            Assert.AreEqual(leftId.GetHashCode(), rightId.GetHashCode());

            Assert.AreEqual(leftId, rightId);
            Assert.AreEqual(rightId, leftId);
            Assert.True(leftId.Equals(rightId));
            Assert.True(leftId.Equals((object)rightId));
            Assert.True(rightId.Equals(leftId));
            Assert.True(rightId.Equals((object)leftId));

            var isEqual1 = leftId == rightId;
            var isEqual2 = rightId == leftId;
            var isNotEqual1 = leftId != rightId;
            var isNotEqual2 = rightId != leftId!;

            Assert.True(isEqual1);
            Assert.True(isEqual2);
            Assert.False(isNotEqual1);
            Assert.False(isNotEqual2);
        }

        [Test]
        public void DocsId_instances_can_be_compared_to_null()
        {
            (new DocsId("value") == null).Should().BeFalse();
            (null == new DocsId("value")).Should().BeFalse();

            (new DocsId("value") != null).Should().BeTrue();
            (null != new DocsId("value")).Should().BeTrue();

            (null == (DocsId?)null).Should().BeTrue();
            (null != (DocsId?)null).Should().BeFalse();
        }
    }
}
