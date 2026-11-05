using System;
using NUnit.Framework;
using OneMoreFloor.Core;

namespace OneMoreFloor.Tests.EditMode
{
    public class SeededRandomTests
    {
        // The output published with PCG32's reference code for seed 42, stream 54. If this
        // ever changes, the same seed would no longer give the same run.
        [Test]
        public void MatchesThePublishedPcg32Numbers()
        {
            var random = new SeededRandom(42UL, 54UL);

            uint[] expected = { 0xa15c02b7u, 0x7b47f409u, 0xba1d3330u, 0x83d2f293u, 0xbfa4784bu, 0xcbed606eu };
            foreach (uint value in expected)
                Assert.That(random.NextUInt(), Is.EqualTo(value));
        }

        [Test]
        public void SameSeed_GivesTheSameNumbers()
        {
            var first = new SeededRandom(123UL);
            var second = new SeededRandom(123UL);

            for (int i = 0; i < 100; i++)
                Assert.That(first.NextUInt(), Is.EqualTo(second.NextUInt()));
        }

        [Test]
        public void DifferentSeeds_GiveDifferentNumbers()
        {
            var first = new SeededRandom(1UL);
            var second = new SeededRandom(2UL);
            int same = 0;

            for (int i = 0; i < 100; i++)
            {
                if (first.NextUInt() == second.NextUInt())
                    same++;
            }

            Assert.That(same, Is.LessThan(100));
        }

        [Test]
        public void NextInt_StaysInRangeAndReachesEveryValue()
        {
            var random = new SeededRandom(7UL);
            var seen = new bool[5];

            for (int i = 0; i < 1000; i++)
            {
                int value = random.NextInt(5);
                Assert.That(value, Is.InRange(0, 4));
                seen[value] = true;
            }

            Assert.That(seen, Has.All.True);
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NextInt_RejectsAnEmptyRange(int maxExclusive)
        {
            var random = new SeededRandom(7UL);

            Assert.Throws<ArgumentOutOfRangeException>(() => random.NextInt(maxExclusive));
        }
    }
}
