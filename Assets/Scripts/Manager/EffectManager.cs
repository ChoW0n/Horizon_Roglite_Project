using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [Header("References")]
    public static EffectManager instance;

    #region Class
    [System.Serializable]
    public class Effect
    {
        public string _name;
        public GameObject _effectPrefab;
        public int _initialPoolSize = 5;
    }

    public class EffectPoolData
    {
        public Queue<GameObject> poolQueue;
        public int currentSize;

        public EffectPoolData(int initial)
        {
            poolQueue = new Queue<GameObject>();
            currentSize = initial;
        }
    }

    #endregion

    #region References
    public Effect[] _effects;

    private Dictionary<string, EffectPoolData> _effectPools;

    #endregion

    #region Unity CallBack Functions
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEffectPools();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Effect
    private void InitializeEffectPools()
    {
        _effectPools = new Dictionary<string, EffectPoolData>();

        foreach (var effect in _effects)
        {
            var poolData = new EffectPoolData(effect._initialPoolSize);

            for (int i = 0; i < effect._initialPoolSize; i++)
            {
                GameObject obj = CreateNewInstance(effect._effectPrefab);
                poolData.poolQueue.Enqueue(obj);
            }

            _effectPools.Add(effect._name, poolData);
        }
    }

    private GameObject CreateNewInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        instance.SetActive(false);
        instance.transform.SetParent(transform);
        return instance;
    }

    private void SetupEffectInstance(GameObject instance, Vector3 position, Quaternion rotation)
    {
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.SetActive(true);
    }

    public void PlayEffect(string effectName, Vector3 position, Quaternion rotation)
    {
        if (!_effectPools.ContainsKey(effectName))
        {
            Debug.LogWarning($"Effect not found: {effectName}");
            return;
        }

        EffectPoolData poolData = _effectPools[effectName];
        GameObject effectInstance;

        if (poolData.poolQueue.Count > 0)
        {
            effectInstance = poolData.poolQueue.Dequeue();
        }
        else
        {
            // 풀에 없으면 바로 생성
            var effectPrefab = _effects.First(e => e._name == effectName)._effectPrefab;
            effectInstance = CreateNewInstance(effectPrefab);
            poolData.currentSize++;
        }

        SetupEffectInstance(effectInstance, position, rotation);
        StartCoroutine(ReturnToPoolAfterDelay(effectInstance, effectName));
    }

    private IEnumerator ReturnToPoolAfterDelay(GameObject instance, string effectName)
    {
        if (instance == null) yield break;

        ParticleSystem particle = instance.GetComponent<ParticleSystem>();
        float duration = particle != null ? particle.main.duration : 1f;

        yield return new WaitForSeconds(duration + 0.5f);

        if (instance != null && _effectPools.ContainsKey(effectName))
        {
            instance.SetActive(false);
            _effectPools[effectName].poolQueue.Enqueue(instance);
        }
    }
}

#endregion