using System;
using NUnit.Framework;
using OneMoreFloor.Core;

namespace OneMoreFloor.Tests.EditMode
{
    public class HeartsPoolTests
    {
        [TestCase(1, 10)]
        [TestCase(2, 20)]
        [TestCase(3, 30)]
        [TestCase(4, 40)]
        public void StartingHearts_GrowWithTeamSize(int players, int expected)
        {
            Assert.That(HeartsPool.StartingHearts(10, players), Is.EqualTo(expected));
        }

        [Test]
        public void Lose_OneMistakeCanCostSeveralHearts()
        {
            var hearts = new HeartsPool(10);

            int lost = hearts.Lose(3);

            Assert.That(lost, Is.EqualTo(3));
            Assert.That(hearts.Remaining, Is.EqualTo(7));
            Assert.That(hearts.IsEmpty, Is.False);
        }

        [Test]
        public void Lose_NeverGoesBelowZero()
        {
            var hearts = new HeartsPool(2);

            int lost = hearts.Lose(5);

            Assert.That(lost, Is.EqualTo(2));
            Assert.That(hearts.Remaining, Is.EqualTo(0));
            Assert.That(hearts.IsEmpty, Is.True);
        }

        [Test]
        public void Lose_ZeroChangesNothing()
        {
            var hearts = new HeartsPool(2);

            Assert.That(hearts.Lose(0), Is.EqualTo(0));
            Assert.That(hearts.Remaining, Is.EqualTo(2));
        }

        [Test]
        public void Lose_NegativeAmountIsRejected()
        {
            var hearts = new HeartsPool(2);

            Assert.Throws<ArgumentOutOfRangeException>(() => hearts.Lose(-1));
            Assert.That(hearts.Remaining, Is.EqualTo(2));
        }

        [TestCase(0, 1)]
        [TestCase(10, 0)]
        public void StartingHearts_RejectsNonsense(int heartsPerPlayer, int players)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => HeartsPool.StartingHearts(heartsPerPlayer, players));
        }
    }
}
