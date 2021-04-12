using NLipsum.Core;
using NUnit.Framework;
using System;
using System.Linq;

namespace PredicateBuilder.Test
{
    public class StringExtensionTest
    {
        private readonly Random _random = new Random(Environment.TickCount);
        private readonly LipsumGenerator _ipsumGen = new LipsumGenerator();

        [Test]
        public void TruncateTest()
        {
            string s = "The quick brown fox jumps over the lazy dog.";
            string actual = s.Truncate(10);

            Assert.AreEqual("The quick ", actual);
        }

        [Test]
        public void Truncate_NoTruncateTest()
        {
            string s = "The quick brown fox jumps over the lazy dog.";
            string actual = s.Truncate(s.Length + 10);

            Assert.AreEqual(s, actual);
        }

        [Test]
        public void Truncate_ZeroLengthTest()
        {
            string s = "The quick brown fox jumps over the lazy dog.";
            string actual = s.Truncate(0);

            Assert.AreEqual(string.Empty, actual);
        }

        [Test]
        public void Truncate_EmptyStringAndZeroLengthTest()
        {
            string s = "";
            string actual = s.Truncate(0);

            Assert.AreEqual(string.Empty, actual);
        }
    }
}
