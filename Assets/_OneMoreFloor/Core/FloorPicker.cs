using System;
using System.Collections.Generic;

namespace OneMoreFloor.Core
{
    /// <summary>
    /// The director: picks the next floor from the catalog using the run's seeded generator.
    /// It avoids repeating the last floor or genre. It only relaxes that when nothing else can run,
    /// first allowing the same genre, then the same floor. So while only one floor exists it repeats,
    /// and the full rule applies as soon as there is an alternative.
    /// </summary>
    public sealed class FloorPicker
    {
        private readonly IReadOnlyList<FloorCandidate> _catalog;
        private readonly DirectorRules _rules;
        private readonly SeededRandom _random;
        private FloorCandidate _last;

        public FloorPicker(IReadOnlyList<FloorCandidate> catalog, DirectorRules rules, SeededRandom random)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _rules = rules ?? throw new ArgumentNullException(nameof(rules));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public FloorPick Pick(int floorNumber, int playerCount)
        {
            int tier = VisualTiers.TierFor(floorNumber, _rules.TierStartFloors);
            List<FloorCandidate> eligible = FindEligible(floorNumber, playerCount, tier);
            if (eligible.Count == 0)
                throw new InvalidOperationException($"No floor can run on floor {floorNumber} with {playerCount} player(s).");

            List<FloorCandidate> options = eligible.FindAll(floor => IsNewFloor(floor) && IsNewGenre(floor));
            if (options.Count == 0)
                options = eligible.FindAll(IsNewFloor);
            if (options.Count == 0)
                options = eligible;

            FloorCandidate chosen = ChooseByWeight(options);
            _last = chosen;

            return new FloorPick(
                chosen.Id,
                VisualTiers.BestRoomTier(tier, chosen.RoomTiers).Value,
                Difficulty.LevelFor(floorNumber, _rules.FloorsPerDifficultyLevel));
        }

        private List<FloorCandidate> FindEligible(int floorNumber, int playerCount, int tier)
        {
            var eligible = new List<FloorCandidate>();
            foreach (FloorCandidate floor in _catalog)
            {
                bool canRun = floor.Weight > 0
                    && floorNumber >= floor.MinFloor
                    && playerCount >= floor.MinPlayers
                    && playerCount <= floor.MaxPlayers
                    && VisualTiers.BestRoomTier(tier, floor.RoomTiers) != null;

                if (canRun)
                    eligible.Add(floor);
            }

            return eligible;
        }

        private bool IsNewFloor(FloorCandidate floor) => _last == null || floor.Id != _last.Id;

        private bool IsNewGenre(FloorCandidate floor) => _last == null || floor.Genre != _last.Genre;

        private FloorCandidate ChooseByWeight(List<FloorCandidate> options)
        {
            int total = 0;
            foreach (FloorCandidate option in options)
                total += option.Weight;

            int roll = _random.NextInt(total);
            foreach (FloorCandidate option in options)
            {
                roll -= option.Weight;
                if (roll < 0)
                    return option;
            }

            throw new InvalidOperationException("The weighted roll fell outside the total weight.");
        }
    }
}
