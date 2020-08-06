﻿// Note: Adapted from https://github.com/ap0llo/changelog/blob/9c789d570199480801ea95d57f425b425b5f1964/src/ChangeLog.Test/EqualityTest.cs

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Grynwald.Extensions.Statiq.Git.Test.Internal
{
    public interface IEqualityTestDataProvider<TTestee>
    {
        IEnumerable<(TTestee left, TTestee right)> GetEqualTestCases();

        IEnumerable<(TTestee left, TTestee right)> GetUnequalTestCases();
    }

    /// <summary>
    /// Base class for tests of <see cref="IEquatable{T}"/> implementations.
    /// </summary>
    public abstract class EqualityTest<TTestee, TDataProvider>
        where TTestee : IEquatable<TTestee>
        where TDataProvider : IEqualityTestDataProvider<TTestee>, new()
    {

        public static IEnumerable<object[]> EqualityTestCases()
        {
            var dataProvider = (IEqualityTestDataProvider<TTestee>)Activator.CreateInstance(typeof(TDataProvider))!;
            foreach (var (left, right) in dataProvider.GetEqualTestCases())
            {
                yield return new object[] { left, right };
            }
        }

        public static IEnumerable<object[]> InequalityTestCases()
        {
            var dataProvider = (IEqualityTestDataProvider<TTestee>)Activator.CreateInstance(typeof(TDataProvider))!;
            foreach (var (left, right) in dataProvider.GetUnequalTestCases())
            {
                yield return new object[] { left, right };
            }
        }

        public static IEnumerable<object[]> SampleInstance()
        {
            var dataProvider = (IEqualityTestDataProvider<TTestee>)Activator.CreateInstance(typeof(TDataProvider))!;
            foreach (var (left, _) in dataProvider.GetUnequalTestCases())
            {
                yield return new object[] { left };
                yield break;
            }

        }


        private static bool TesteeImplementsEqualitsOperators =>
            typeof(TTestee).GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public) != null;


        [Theory]
        [TestCaseSource(nameof(EqualityTestCases))]
        public void Comparision_of_two_equal_instances_yield_expected_result(TTestee left, TTestee right)
        {
            // instances must be equal to themselves
            Assert.AreEqual(left, left);
            Assert.AreEqual(right, right);

            // hash code must be equal
            Assert.AreEqual(left.GetHashCode(), right.GetHashCode());

            Assert.AreEqual(left, right);
            Assert.AreEqual(right, left);
            Assert.True(left.Equals(right));
            Assert.True(left.Equals((object)right));
            Assert.True(right.Equals(left));
            Assert.True(right.Equals((object)left));

            if (TesteeImplementsEqualitsOperators)
            {
                var opEquality = typeof(TTestee).GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public);
                var opInequality = typeof(TTestee).GetMethod("op_Inequality", BindingFlags.Static | BindingFlags.Public);
                Assert.NotNull(opEquality);
                Assert.NotNull(opInequality);

                var isEqual1 = (bool)opEquality!.Invoke(null, new object[] { left, right })!;
                var isEqual2 = (bool)opEquality!.Invoke(null, new object[] { right, left })!;
                var isNotEqual1 = (bool)opInequality!.Invoke(null, new object[] { left, right })!;
                var isNotEqual2 = (bool)opInequality!.Invoke(null, new object[] { right, left })!;

                Assert.True(isEqual1);
                Assert.True(isEqual2);
                Assert.False(isNotEqual1);
                Assert.False(isNotEqual2);
            }
        }

        [Theory]
        [TestCaseSource(nameof(InequalityTestCases))]
        public void Comparision_of_two_unequal_instances_yield_expected_result(TTestee left, TTestee right)
        {
            Assert.AreNotEqual(left, right);
            Assert.AreNotEqual(right, left);
            Assert.False(left.Equals(right));
            Assert.False(left.Equals((object)right));
            Assert.False(right.Equals(left));
            Assert.False(right.Equals((object)left));


            if (TesteeImplementsEqualitsOperators)
            {
                var opEquality = typeof(TTestee).GetMethod("op_Equality", BindingFlags.Static | BindingFlags.Public);
                var opInequality = typeof(TTestee).GetMethod("op_Inequality", BindingFlags.Static | BindingFlags.Public);
                Assert.NotNull(opEquality);
                Assert.NotNull(opInequality);

                var isEqual1 = (bool)opEquality!.Invoke(null, new object[] { left, right })!;
                var isEqual2 = (bool)opEquality!.Invoke(null, new object[] { right, left })!;
                var isNotEqual1 = (bool)opInequality!.Invoke(null, new object[] { left, right })!;
                var isNotEqual2 = (bool)opInequality!.Invoke(null, new object[] { right, left })!;

                Assert.False(isEqual1);
                Assert.False(isEqual2);
                Assert.True(isNotEqual1);
                Assert.True(isNotEqual2);
            }
        }

        [Theory]
        [TestCaseSource(nameof(SampleInstance))]
        public void Equals_retuns_false_if_argument_is_of_a_different_type(TTestee sut)
        {
            Assert.False(sut.Equals(new object()));
        }


    }
}