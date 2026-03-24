using UnityEngine;
using System.Collections.Generic;

namespace Linksaurus.Spawning
{
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public GameObject GetFromPool(GameObject prefab)
        {
            string key = prefab.name;

            if (!_pools.ContainsKey(key))
            {
                _pools.Add(key, new Queue<GameObject>());
            }

            if (_pools[key].Count > 0)
            {
                GameObject obj = _pools[key].Dequeue();
                obj.SetActive(true);
                return obj;
            }

            GameObject newObj = Instantiate(prefab);
            newObj.name = prefab.name; // Keep name for pooling key
            return newObj;
        }

        public void ReturnToPool(GameObject obj)
        {
            string key = obj.name;
            obj.SetActive(false);

            if (!_pools.ContainsKey(key))
            {
                _pools.Add(key, new Queue<GameObject>());
            }

            _pools[key].Enqueue(obj);
        }

        // Pre-warm logic will be called by SpawnManager or here on Start
        public void PreWarm(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.name = prefab.name;
                ReturnToPool(obj);
            }
        }
    }
}
