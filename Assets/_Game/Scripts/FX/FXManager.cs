using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GraveyardHunter.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace GraveyardHunter.FX
{
    public class FXManager : MonoBehaviour
    {
        [System.Serializable]
        public class FXEntry
        {
            public string Name;
            public GameObject Prefab;
            public float Duration = 2f;
        }

        [TableList]
        [SerializeField] private List<FXEntry> _fxLibrary;

        private ObjectPool _pool;

        private void Awake()
        {
            ServiceLocator.Register(this);
            EventBus.Subscribe<SpawnFXEvent>(OnSpawnFXEvent);

            _pool = ServiceLocator.Get<ObjectPool>();
            if (_pool != null)
            {
                foreach (var entry in _fxLibrary)
                {
                    if (entry.Prefab != null)
                        _pool.RegisterPrefab(entry.Name, entry.Prefab);
                }
            }
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<FXManager>();
            EventBus.Unsubscribe<SpawnFXEvent>(OnSpawnFXEvent);
        }

        public void SpawnFX(string name, Vector3 pos, Quaternion rot)
        {
            if (_pool == null)
            {
                Debug.LogWarning("[FXManager] ObjectPool not available.");
                return;
            }

            var entry = _fxLibrary.FirstOrDefault(e => e.Name == name);
            if (entry == null)
            {
                Debug.LogWarning($"[FXManager] FX not found: {name}");
                return;
            }

            var obj = _pool.Spawn(name, pos, rot);
            if (obj != null)
                StartCoroutine(DespawnAfterDelay(name, obj, entry.Duration));
        }

        private IEnumerator DespawnAfterDelay(string name, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null && _pool != null)
                _pool.Despawn(name, obj);
        }

        private void OnSpawnFXEvent(SpawnFXEvent evt)
        {
            SpawnFX(evt.FXName, evt.Position, evt.Rotation);
        }
    }
}
