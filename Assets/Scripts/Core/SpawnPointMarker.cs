using UnityEngine;

namespace GravityDefenders
{
    /// <summary>
    /// Empty marker component used by WaveManager to auto-discover enemy spawn points.
    /// Attach this to any GameObject whose Transform should be used as an enemy spawn location.
    /// Optionally you can also tag the object with the tag configured in WaveManager (default 'EnemySpawn').
    /// </summary>
    public class SpawnPointMarker : MonoBehaviour { }
}
