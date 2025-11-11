using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    public class MapZone : MonoBehaviour
    {
        [Header("Central Zone")]
        [SerializeField] private bool designateAsCentral;

        [Header("Resource Veins")]
        [SerializeField] private GameObject resourceVeinAPrefab;
        [SerializeField] private GameObject resourceVeinBPrefab;
        [SerializeField] private List<Transform> veinSpawnPoints = new List<Transform>();
        [SerializeField, Range(0f, 1f)] private float veinSpawnChance = 0.5f;

    [Header("Interaction")]
    [SerializeField] private bool allowMouseInteraction = true;
    [SerializeField] private bool autoExpandOnClick = true;

    [Header("Movement Animation")]
    [SerializeField] private float repositionDuration = 1.25f;
    [SerializeField] private AnimationCurve repositionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public bool IsShielded { get; private set; }

        private bool hasSpawnedVein;
    private Coroutine repositionRoutine;

    [System.Serializable]
    private class ZoneUnityEvent : UnityEvent<MapZone> { }

    [System.Serializable]
    private class ZonePositionUnityEvent : UnityEvent<MapZone, Vector3> { }

    [SerializeField] private ZoneUnityEvent zoneClickedEvent = new ZoneUnityEvent();
    [SerializeField] private ZoneUnityEvent shieldStateChangedEvent = new ZoneUnityEvent();
    [SerializeField] private ZoneUnityEvent shieldExpansionSucceededEvent = new ZoneUnityEvent();
    [SerializeField] private ZoneUnityEvent shieldExpansionFailedEvent = new ZoneUnityEvent();
    [SerializeField] private ZonePositionUnityEvent repositionCompletedEvent = new ZonePositionUnityEvent();

    public UnityEvent<MapZone> ZoneClickedEvent => zoneClickedEvent;
    public UnityEvent<MapZone> ShieldStateChangedEvent => shieldStateChangedEvent;
    public UnityEvent<MapZone> ShieldExpansionSucceededEvent => shieldExpansionSucceededEvent;
    public UnityEvent<MapZone> ShieldExpansionFailedEvent => shieldExpansionFailedEvent;
    public UnityEvent<MapZone, Vector3> RepositionCompletedEvent => repositionCompletedEvent;

        public bool IsCentralCandidate()
        {
            if (designateAsCentral) return true;
            return Vector3.Distance(transform.position, Vector3.zero) < 1f;
        }

        public void TransformTo(Vector3 position)
        {
            if (repositionRoutine != null)
            {
                StopCoroutine(repositionRoutine);
            }

            if (repositionDuration <= 0f)
            {
                transform.position = position;
                repositionCompletedEvent.Invoke(this, position);
                return;
            }

            repositionRoutine = StartCoroutine(RepositionRoutine(position));
        }

        public void SetShielded(bool isShielded)
        {
            if (IsShielded == isShielded) return;

            IsShielded = isShielded;
            shieldStateChangedEvent.Invoke(this);

            if (IsShielded && !hasSpawnedVein)
            {
                TrySpawnResourceVein();
            }
        }

        public void RequestShieldExpansion()
        {
            if (MapManager.Instance == null) return;

            bool success = MapManager.Instance.TryExpandShieldToZone(this);
            if (success)
            {
                shieldExpansionSucceededEvent.Invoke(this);
            }
            else
            {
                shieldExpansionFailedEvent.Invoke(this);
            }
        }

        void OnMouseDown()
        {
            if (!allowMouseInteraction) return;
            zoneClickedEvent.Invoke(this);

            if (autoExpandOnClick && !IsShielded)
            {
                RequestShieldExpansion();
            }
        }

        private void TrySpawnResourceVein()
        {
            if (veinSpawnPoints.Count == 0) return;
            if (resourceVeinAPrefab == null && resourceVeinBPrefab == null) return;

            if (Random.value >= veinSpawnChance) return;

            hasSpawnedVein = true;

            Transform spawnPoint = veinSpawnPoints[Random.Range(0, veinSpawnPoints.Count)];
            GameObject prefabToSpawn = DecideVeinPrefab();

            if (prefabToSpawn == null) return;

            Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            Debug.Log($"Resource vein spawned in {name}.");
        }

        private GameObject DecideVeinPrefab()
        {
            if (resourceVeinAPrefab == null) return resourceVeinBPrefab;
            if (resourceVeinBPrefab == null) return resourceVeinAPrefab;
            return Random.value < 0.5f ? resourceVeinAPrefab : resourceVeinBPrefab;
        }

        private IEnumerator RepositionRoutine(Vector3 targetPosition)
        {
            Vector3 startPosition = transform.position;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, repositionDuration);

            while (elapsed < duration)
            {
                float t = repositionCurve != null ? repositionCurve.Evaluate(elapsed / duration) : (elapsed / duration);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
            repositionRoutine = null;
            repositionCompletedEvent.Invoke(this, targetPosition);
        }
    }
}

