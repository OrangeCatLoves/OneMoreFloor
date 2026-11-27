using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

namespace OneMoreFloor.FloorSDK
{
    /// <summary>What a floor can do with a player character, without knowing how the game builds one.</summary>
    public interface IFloorPlayer
    {
        /// <summary>The network connection id of the player who owns this character.</summary>
        int ClientId { get; }

        /// <summary>True on the machine of the player who controls this character.</summary>
        bool IsLocal { get; }

        Transform Body { get; }

        /// <summary>The player's controls (rebindable). Floors read their actions from it, e.g. Interact and Choice1-4.</summary>
        InputActionAsset Controls { get; }

        /// <summary>Local player only: stand still at this spot facing this way, with the mouse cursor free (e.g. at a podium).</summary>
        void HoldAt(Vector3 position, float yaw);

        /// <summary>Local player only: back to normal movement and mouse look.</summary>
        void Release();

        bool IsHeld { get; }
    }

    /// <summary>Every player character in the session, on every machine. Characters register themselves.</summary>
    public static class FloorPlayers
    {
        private static readonly List<IFloorPlayer> Players = new List<IFloorPlayer>();

        public static IReadOnlyList<IFloorPlayer> All
        {
            get
            {
                RemoveDestroyed();
                return Players;
            }
        }

        public static IFloorPlayer Local
        {
            get
            {
                RemoveDestroyed();
                foreach (IFloorPlayer player in Players)
                    if (player.IsLocal)
                        return player;
                return null;
            }
        }

        public static IFloorPlayer ByClient(int clientId)
        {
            RemoveDestroyed();
            foreach (IFloorPlayer player in Players)
                if (player.ClientId == clientId)
                    return player;
            return null;
        }

        public static void Register(IFloorPlayer player)
        {
            if (!Players.Contains(player))
                Players.Add(player);
        }

        public static void Unregister(IFloorPlayer player) => Players.Remove(player);

        // A character destroyed without leaving the network properly (a scene unloading, the session shutting
        // down) must not count as a player any more. Unity's == null is true for destroyed objects.
        private static void RemoveDestroyed() =>
            Players.RemoveAll(player => player == null || (player is Object unityObject && unityObject == null));
    }
}
