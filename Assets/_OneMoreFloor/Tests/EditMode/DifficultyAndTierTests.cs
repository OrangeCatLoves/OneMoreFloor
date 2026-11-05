using System;
using NUnit.Framework;
using OneMoreFloor.Core;

namespace OneMoreFloor.Tests.EditMode
{
    public class DifficultyAndTierTests
    {
        private static readonly int[] DefaultTierStarts = { 1, 11, 21 };

        [TestCase(1, 1)]
        [TestCase(5, 1)]
        [TestCase(6, 2)]
        [TestCase(7, 2)]
        [TestCase(10, 2)]
        [TestCase(11, 3)]
        [TestCase(15, 3)]
        [TestCase(16, 4)]
        public void DifficultyLevel_RisesEveryFiveFloors(int floor, int expectedLevel)
        {
            Assert.That(Difficulty.LevelFor(floor, 5), Is.EqualTo(expectedLevel));
        }

        [Test]
        public void DifficultyLevel_RejectsFloorsBelowOne()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Difficulty.LevelFor(0, 5));
        }

        [TestCase(1, 1)]
        [TestCase(10, 1)]
        [TestCase(11, 2)]
        [TestCase(20, 2)]
        [TestCase(21, 3)]
        [TestCase(100, 3)]
        public void VisualTier_FollowsTheFloorRanges(int floor, int expectedTier)
        {
            Assert.That(VisualTiers.TierFor(floor, DefaultTierStarts), Is.EqualTo(expectedTier));
        }

        [Test]
        public void BestRoomTier_UsesTheHighestTierAtOrBelowTheCurrentOne()
        {
            Assert.That(VisualTiers.BestRoomTier(3, new[] { 1, 2 }), Is.EqualTo(2));
            Assert.That(VisualTiers.BestRoomTier(2, new[] { 1, 2, 3 }), Is.EqualTo(2));
            Assert.That(VisualTiers.BestRoomTier(1, new[] { 1 }), Is.EqualTo(1));
        }

        [Test]
        public void BestRoomTier_IsNullWhenTheFloorOnlyHasHigherRooms()
        {
            Assert.That(VisualTiers.BestRoomTier(1, new[] { 3 }), Is.Null);
        }

        [Test]
        public void DirectorRules_RejectBadTierRanges()
        {
            Assert.Throws<ArgumentException>(() => new DirectorRules(5, new[] { 2, 11 }), "First tier must start at floor 1");
            Assert.Throws<ArgumentException>(() => new DirectorRules(5, new[] { 1, 11, 11 }), "Tiers must increase");
            Assert.Throws<ArgumentException>(() => new DirectorRules(5, new int[0]), "At least one tier");
            Assert.Throws<ArgumentOutOfRangeException>(() => new DirectorRules(0, DefaultTierStarts));
        }
    }
}
