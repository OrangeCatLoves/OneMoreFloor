using System;
using FishNet.Object;
using OneMoreFloor.Core;

namespace OneMoreFloor.FloorSDK
{
    /// <summary>
    /// What a floor may ask of the run while it plays (host only). Floors never change hearts or the score
    /// themselves: they report mistakes and say when they're cleared.
    /// </summary>
    public interface IFloorRun
    {
        int FloorNumber { get; }

        /// <summary>Difficulty level for this floor number (1, 2, 3, ...).</summary>
        int Level { get; }

        /// <summary>True while the floor is being played: hearts can be lost and the floor can be cleared.</summary>
        bool IsPlaying { get; }

        /// <summary>A random generator for this floor, from the run's seed. Floors use only this one.</summary>
        SeededRandom Random { get; }

        /// <summary>
        /// Reports a mistake. The run applies it at once and may end (hearts reach 0), which stops the floor.
        /// <paramref name="reason"/> is a short id for stats; <paramref name="playerClientId"/> is the player
        /// who made it, or -1 for the whole team.
        /// </summary>
        void LoseHearts(int amount, string reason, int playerClientId = -1);

        /// <summary>Reports that the team has cleared the floor.</summary>
        void Cleared();

        /// <summary>
        /// Floor state that lasts for the whole run, e.g. Trivia's questions already asked. Created by
        /// <paramref name="create"/> the first time the key is used in a run.
        /// </summary>
        T RunMemory<T>(string key, Func<T> create) where T : class;
    }

    /// <summary>
    /// Base class of a floor's networked controller (docs/PROJECT_CONTEXT.md, section 4). The host decides
    /// everything; clients only send requests. The run calls BeginPlay when the title card ends and
    /// EndPlay when the floor is cleared or the run ends, so a floor must stop cleanly at any moment.
    /// </summary>
    public abstract class NetworkFloor : NetworkBehaviour
    {
        /// <summary>Host only: the title card has ended and play starts.</summary>
        public abstract void BeginPlay(IFloorRun run);

        /// <summary>Host only: the floor is cleared, or the run ended. Stop timers and hand control back to players.</summary>
        public abstract void EndPlay();
    }
}
