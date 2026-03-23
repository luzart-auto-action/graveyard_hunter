using System.Collections.Generic;
using UnityEngine;

namespace GraveyardHunter.Core
{
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class PoolEntry
        {
            public string Name;
            public GameObject Prefab;
            public int InitialCount = 5;
        }

        [SerializeField] private List<PoolEntry> _poolEntries = new List<PoolEntry>();

        private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> _prefabMap = new Dictionary<string, GameObject>();

        private void Awake()
        {
            ServiceLocator.Register(this);
            InitializePools();
        }

        private void OnDestroy()
        {
            ServiceLocator.Unregister<ObjectPool>();
        }

        private void InitializePools()
        {
            foreach (var entry in _poolEntries)
            {
                if (entry.Prefab == null) continue;

                _prefabMap[entry.Name] = entry.Prefab;
                _pools[entry.Name] = new Queue<GameObject>();

                for (int i = 0; i < entry.InitialCount; i++)
                {
                    var obj = CreateNewObject(entry.Name);
                    obj.SetActive(false);
                    _pools[entry.Name].Enqueue(obj);
                }
            }
        }

        private GameObject CreateNewObject(string poolName)
        {
            if (!_prefabMap.TryGetValue(poolName, out var prefab)) return null;

            var obj = Instantiate(prefab, transform);
            obj.name = $"{poolName}_pooled";
            return obj;
        }

        public GameObject Spawn(string poolName, Vector3 position, Quaternion rotation)
        {
            GameObject obj = null;

            if (_pools.ContainsKey(poolName) && _pools[poolName].Count > 0)
            {
                obj = _pools[poolName].Dequeue();
            }
            else if (_prefabMap.ContainsKey(poolName))
            {
                obj = CreateNewObject(poolName);
            }

            if (obj == null)
            {
                Debug.LogWarning($"[ObjectPool] Pool '{poolName}' not found.");
                return null;
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        public void Despawn(string poolName, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(transform);

            if (!_pools.ContainsKey(poolName))
                _pools[poolName] = new Queue<GameObject>();

            _pools[poolName].Enqueue(obj);
        }

        public void ClearPool(string poolName)
        {
            if (!_pools.ContainsKey(poolName)) return;

            while (_pools[poolName].Count > 0)
            {
                var obj = _pools[poolName].Dequeue();
                if (obj != null) Destroy(obj);
            }
        }

        public void ClearAllPools()
        {
            foreach (var key in _pools.Keys)
            {
                ClearPool(key);
            }
        }

        public void RegisterPrefab(string poolName, GameObject prefab, int initialCount = 5)
        {
            if (_prefabMap.ContainsKey(poolName)) return;

            _prefabMap[poolName] = prefab;
            _pools[poolName] = new Queue<GameObject>();

            for (int i = 0; i < initialCount; i++)
            {
                var obj = CreateNewObject(poolName);
                obj.SetActive(false);
                _pools[poolName].Enqueue(obj);
            }
        }
    }
}
