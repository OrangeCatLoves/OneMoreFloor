using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OneMoreFloor.Core;

namespace OneMoreFloor.Tests.EditMode
{
    public class FloorPickerTests
    {
        private static readonly DirectorRules Rules = new DirectorRules(5, new[] { 1, 11, 21 });

        private static FloorCandidate Floor(
            string id,
            string genre = null,
            int minFloor = 1,
            int minPlayers = 1,
            int maxPlayers = 4,
            int weight = 1,
            int[] roomTiers = null)
        {
            return new FloorCandidate(id, genre ?? id + "_genre", minFloor, minPlayers, maxPlayers, weight, roomTiers ?? new[] { 1 });
        }

        private static FloorPicker Picker(ulong seed, params FloorCandidate[] catalog)
        {
            return new FloorPicker(catalog, Rules, new SeededRandom(seed));
        }

        private static List<string> PickMany(FloorPicker picker, int count, int players = 1)
        {
            var picks = new List<string>();
            for (int floor = 1; floor <= count; floor++)
                picks.Add(picker.Pick(floor, players).FloorId);
            return picks;
        }

        private static FloorCandidate[] ThreeFloors() => new[] { Floor("a"), Floor("b"), Floor("c") };

        [Test]
        public void SameSeed_GivesTheSameFloorOrder()
        {
            List<string> first = PickMany(Picker(99UL, ThreeFloors()), 200);
            List<string> second = PickMany(Picker(99UL, ThreeFloors()), 200);

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void DifferentSeeds_GiveDifferentFloorOrders()
        {
            List<string> first = PickMany(Picker(1UL, ThreeFloors()), 200);
            List<string> second = PickMany(Picker(2UL, ThreeFloors()), 200);

            Assert.That(second, Is.Not.EqualTo(first));
        }

        [Test]
        public void NeverRepeatsAFloorWhenThereIsAnAlternative()
        {
            List<string> picks = PickMany(Picker(5UL, ThreeFloors()), 500);

            for (int i = 1; i < picks.Count; i++)
                Assert.That(picks[i], Is.Not.EqualTo(picks[i - 1]));
        }

        [Test]
        public void NeverRepeatsAGenreWhenThereIsAnAlternative()
        {
            FloorCandidate[] catalog =
            {
                Floor("a", genre: "knowledge"),
                Floor("b", genre: "knowledge"),
                Floor("c", genre: "panic"),
                Floor("d", genre: "panic"),
            };
            FloorPicker picker = Picker(5UL, catalog);
            string lastGenre = null;

            for (int floor = 1; floor <= 500; floor++)
            {
                string id = picker.Pick(floor, 1).FloorId;
                string genre = catalog.First(f => f.Id == id).Genre;
                Assert.That(genre, Is.Not.EqualTo(lastGenre));
                lastGenre = genre;
            }
        }

        // The owner's exemption while only Trivia exists.
        [Test]
        public void WithOnlyOneFloor_ItRepeats()
        {
            List<string> picks = PickMany(Picker(5UL, Floor("trivia")), 20);

            Assert.That(picks, Has.All.EqualTo("trivia"));
        }

        [Test]
        public void WithTwoFloorsOfTheSameGenre_ItStillAvoidsRepeatingAFloor()
        {
            List<string> picks = PickMany(Picker(5UL, Floor("a", genre: "same"), Floor("b", genre: "same")), 100);

            for (int i = 1; i < picks.Count; i++)
                Assert.That(picks[i], Is.Not.EqualTo(picks[i - 1]));
        }

        [Test]
        public void TheNoRepeatRuleReturnsOnceASecondFloorUnlocks()
        {
            FloorPicker picker = Picker(5UL, Floor("a"), Floor("b", minFloor: 4));
            var picks = new List<string>();
            for (int floor = 1; floor <= 50; floor++)
                picks.Add(picker.Pick(floor, 1).FloorId);

            Assert.That(picks.Take(3), Has.All.EqualTo("a"));
            for (int i = 3; i < picks.Count; i++)
                Assert.That(picks[i], Is.Not.EqualTo(picks[i - 1]));
        }

        [Test]
        public void AFloorOnlyAppearsFromItsMinFloor()
        {
            FloorPicker picker = Picker(5UL, Floor("a"), Floor("b"), Floor("late", minFloor: 6));

            for (int floor = 1; floor <= 5; floor++)
                Assert.That(picker.Pick(floor, 1).FloorId, Is.Not.EqualTo("late"));

            var later = new List<string>();
            for (int floor = 6; floor <= 100; floor++)
                later.Add(picker.Pick(floor, 1).FloorId);

            Assert.That(later, Has.Member("late"));
        }

        [Test]
        public void AFloorIsSkippedOutsideItsPlayerLimits()
        {
            List<string> alone = PickMany(Picker(5UL, Floor("any"), Floor("teams", minPlayers: 2), Floor("solo", maxPlayers: 1)), 100, players: 1);
            Assert.That(alone, Has.None.EqualTo("teams"));

            List<string> group = PickMany(Picker(5UL, Floor("any"), Floor("teams", minPlayers: 2), Floor("solo", maxPlayers: 1)), 100, players: 2);
            Assert.That(group, Has.None.EqualTo("solo"));
        }

        [Test]
        public void AFloorWithZeroWeightNeverAppears()
        {
            List<string> picks = PickMany(Picker(5UL, Floor("a"), Floor("b"), Floor("off", weight: 0)), 200);

            Assert.That(picks, Has.None.EqualTo("off"));
        }

        [Test]
        public void AHigherWeightAppearsMoreOften()
        {
            List<string> picks = PickMany(Picker(5UL, Floor("common", weight: 9), Floor("x"), Floor("y"), Floor("z")), 2000);
            int Count(string id) => picks.Count(p => p == id);

            Assert.That(Count("common"), Is.GreaterThan(Count("x")));
            Assert.That(Count("common"), Is.GreaterThan(Count("y")));
            Assert.That(Count("common"), Is.GreaterThan(Count("z")));
        }

        [Test]
        public void UsesTheHighestRoomAtOrBelowTheCurrentTier()
        {
            FloorPicker picker = Picker(5UL, Floor("a", roomTiers: new[] { 1, 2 }));

            Assert.That(picker.Pick(5, 1).RoomTier, Is.EqualTo(1));
            Assert.That(picker.Pick(15, 1).RoomTier, Is.EqualTo(2));
            Assert.That(picker.Pick(25, 1).RoomTier, Is.EqualTo(2), "Tier 3, but the floor has no tier 3 room");
        }

        [Test]
        public void AFloorWithOnlyHigherRoomsWaitsForItsTier()
        {
            FloorPicker picker = Picker(5UL, Floor("a"), Floor("world", roomTiers: new[] { 3 }));

            for (int floor = 1; floor <= 20; floor++)
                Assert.That(picker.Pick(floor, 1).FloorId, Is.EqualTo("a"));

            Assert.That(picker.Pick(21, 1).FloorId, Is.EqualTo("world"));
        }

        [Test]
        public void GivesTheDifficultyLevelForTheFloorNumber()
        {
            FloorPicker picker = Picker(5UL, Floor("a"));

            Assert.That(picker.Pick(1, 1).DifficultyLevel, Is.EqualTo(1));
            Assert.That(picker.Pick(7, 1).DifficultyLevel, Is.EqualTo(2));
        }

        [Test]
        public void ThrowsWhenNoFloorCanRun()
        {
            FloorPicker picker = Picker(5UL, Floor("late", minFloor: 10));

            Assert.Throws<InvalidOperationException>(() => picker.Pick(1, 1));
        }
    }
}
