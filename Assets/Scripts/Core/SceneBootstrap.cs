using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GravityDefenders
{
    [DefaultExecutionOrder(-10000)]
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("Auto Creation")]
        [SerializeField] private bool ensureManagers = true;
        [SerializeField] private bool validateOnStart = true;

        [Header("Validation")]
        [SerializeField] private bool requireMainCamera = true;
        [SerializeField] private bool requireCinemachineBrainOnMainCamera = true;
        [SerializeField] private bool validateEnemyTagExists = true;
        [SerializeField] private bool validateCollidersOnClickable = true;
        [SerializeField] private bool validateMiningLayerMask = true;

        [System.Serializable]
        private class StringEvent : UnityEvent<string> { }

        [Header("Events")]
        [SerializeField] private StringEvent validationIssueRaised = new StringEvent();
        [SerializeField] private UnityEvent validationCompleted = new UnityEvent();

        public UnityEvent<string> ValidationIssueRaised => validationIssueRaised;
        public UnityEvent ValidationCompleted => validationCompleted;

        void Awake()
        {
            if (ensureManagers)
            {
                EnsureManagers();
            }
        }

        void Start()
        {
            if (validateOnStart)
            {
                ValidateScene();
            }
        }

        private void EnsureManagers()
        {
            // Creation order to satisfy common dependencies before GameManager.Start
            EnsureSingleton<ResourceManager>();
            EnsureSingleton<MetaProgressionManager>();
            EnsureSingleton<UpgradeManager>();
            EnsureSingleton<MapManager>();
            EnsureSingleton<MiningManager>();
            EnsureSingleton<TurretBuilder>();
            EnsureSingleton<WaveManager>();
            EnsureSingleton<GameManager>();
        }

        private void EnsureSingleton<T>() where T : Component
        {
            T existing = FindFirstObjectByType<T>();
            if (existing != null) return;

            GameObject go = new GameObject(typeof(T).Name);
            go.AddComponent<T>();
            LogInfo($"Created missing manager: {typeof(T).Name}");
        }

        public void ValidateScene()
        {
            // Input System check via reflection to avoid hard dependency
            var mouseType = Type.GetType("UnityEngine.InputSystem.Mouse, Unity.InputSystem");
            if (mouseType == null)
            {
                LogError("Input System not detected. Ensure package installed and Active Input Handling = Input System.");
            }

            // Camera checks
            if (requireMainCamera)
            {
                if (Camera.main == null)
                {
                    LogError("No MainCamera found. Tag a camera as MainCamera.");
                }
                else if (requireCinemachineBrainOnMainCamera)
                {
                    var brainType = Type.GetType("Cinemachine.CinemachineBrain, Cinemachine");
                    if (brainType == null)
                    {
                        LogWarning("Cinemachine not detected. Install Cinemachine 3.1.5 or disable this check.");
                    }
                    else if (Camera.main.GetComponent(brainType) == null)
                    {
                        LogWarning("MainCamera missing CinemachineBrain component.");
                    }
                }
            }

            // Tag existence check: runtime-safe try
            if (validateEnemyTagExists)
            {
                try
                {
                    _ = GameObject.FindGameObjectsWithTag("Enemy");
                }
                catch (UnityException)
                {
                    LogError("Tag 'Enemy' is not defined in Tag Manager.");
                }
            }

            // Collider checks on clickable components
            if (validateCollidersOnClickable)
            {
                foreach (var zone in FindObjectsByType<MapZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (zone.GetComponent<Collider>() == null)
                    {
                        LogError($"MapZone '{zone.name}' has no Collider. OnMouseDown won't fire.");
                    }
                }
                foreach (var slot in FindObjectsByType<TurretBuildSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (slot.GetComponent<Collider>() == null)
                    {
                        LogError($"TurretBuildSlot '{slot.name}' has no Collider. OnMouseDown won't fire.");
                    }
                }
            }

            // Mining layer mask coverage
            if (validateMiningLayerMask)
            {
                var mining = FindFirstObjectByType<MiningManager>();
                if (mining != null)
                {
                    foreach (var vein in FindObjectsByType<ResourceVein>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    {
                        var col = vein.GetComponent<Collider>();
                        if (col == null) continue;
                        int layer = col.gameObject.layer;
                        if ((miningLayerMask(mining) & (1 << layer)) == 0)
                        {
                            LogWarning($"ResourceVein '{vein.name}' on layer {LayerMask.LayerToName(layer)} not included in MiningManager.miningLayerMask.");
                        }
                    }
                }
            }

            validationCompleted.Invoke();
        }

        private static int miningLayerMask(MiningManager m)
        {
            // Access private serialized field through reflection if needed; otherwise require public accessor
            var field = typeof(MiningManager).GetField("miningLayerMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                var mask = (LayerMask)field.GetValue(m);
                return mask.value;
            }
            return ~0; // assume all if not accessible
        }

        private void LogInfo(string msg)
        {
            Debug.Log($"[SceneBootstrap] {msg}");
            validationIssueRaised.Invoke(msg);
        }

        private void LogWarning(string msg)
        {
            Debug.LogWarning($"[SceneBootstrap] {msg}");
            validationIssueRaised.Invoke(msg);
        }

        private void LogError(string msg)
        {
            Debug.LogError($"[SceneBootstrap] {msg}");
            validationIssueRaised.Invoke(msg);
        }
    }
}
