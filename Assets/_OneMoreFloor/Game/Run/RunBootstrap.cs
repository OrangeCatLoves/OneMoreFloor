using System.Collections;
using FishNet;
using UnityEngine;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// Put in a playable scene: once this machine is hosting, it spawns the run manager, which then runs
    /// the floors. Joining players get the run manager from the host and do nothing here.
    /// </summary>
    public sealed class RunBootstrap : MonoBehaviour
    {
        [SerializeField]
        private RunManager runManagerPrefab;

        private IEnumerator Start()
        {
            while (InstanceFinder.NetworkManager == null || !InstanceFinder.IsServerStarted)
            {
                if (InstanceFinder.NetworkManager != null && InstanceFinder.IsClientOnlyStarted)
                    yield break;
                yield return null;
            }

            if (RunManager.Instance != null)
                yield break;

            RunManager run = Instantiate(runManagerPrefab);
            InstanceFinder.ServerManager.Spawn(run.gameObject);
        }
    }
}
