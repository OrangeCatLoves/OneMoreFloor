using System.Collections;
using FishNet;
using FishNet.Managing;
using UnityEngine;
#if UNITY_EDITOR
using Unity.Multiplayer.PlayMode;
#endif

namespace OneMoreFloor.Game
{
    /// <summary>
    /// Starts a session when the scene loads. Solo is a local host with one player, the same code
    /// path as multiplayer. In the editor's multiplayer testing, the main editor hosts and each
    /// virtual player joins it. Steam lobbies replace this for real co-op later.
    /// </summary>
    public sealed class LocalSessionStarter : MonoBehaviour
    {
        // A virtual player can start before the host is listening, so it keeps trying.
        private const float RetrySeconds = 2f;

        private static bool IsHost
        {
            get
            {
#if UNITY_EDITOR
                return CurrentPlayer.IsMainEditor;
#else
                return true;
#endif
            }
        }

        private IEnumerator Start()
        {
            NetworkManager network = InstanceFinder.NetworkManager;

            if (IsHost)
                network.ServerManager.StartConnection();

            var wait = new WaitForSeconds(RetrySeconds);
            while (true)
            {
                if (!network.ClientManager.Started)
                    network.ClientManager.StartConnection();

                yield return wait;
            }
        }
    }
}
