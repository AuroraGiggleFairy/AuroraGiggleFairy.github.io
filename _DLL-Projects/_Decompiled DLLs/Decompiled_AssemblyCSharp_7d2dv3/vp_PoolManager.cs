using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class vp_PoolManager : MonoBehaviour
{
	[Serializable]
	public class vp_CustomPooledObject
	{
		public GameObject Prefab;

		public int Buffer = 15;

		public int MaxAmount = 25;
	}

	public int MaxAmount = 25;

	public bool PoolOnDestroy = true;

	public List<GameObject> IgnoredPrefabs = new List<GameObject>();

	public List<vp_CustomPooledObject> CustomPrefabs = new List<vp_CustomPooledObject>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform m_Transform;

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, List<UnityEngine.Object>> m_AvailableObjects = new Dictionary<string, List<UnityEngine.Object>>();

	[NonSerialized]
	[PublicizedFrom(EAccessModifier.Protected)]
	public Dictionary<string, List<UnityEngine.Object>> m_UsedObjects = new Dictionary<string, List<UnityEngine.Object>>();

	[NonSerialized]
	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public static vp_PoolManager m_Instance;

	[Preserve]
	public static vp_PoolManager Instance => m_Instance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		m_Instance = this;
		m_Transform = base.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		foreach (vp_CustomPooledObject customPrefab in CustomPrefabs)
		{
			AddObjects(customPrefab.Prefab, Vector3.zero, Quaternion.identity, customPrefab.Buffer);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		vp_GlobalEventReturn<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>.Register("vp_PoolManager Instantiate", InstantiateInternal);
		vp_GlobalEvent<UnityEngine.Object, float>.Register("vp_PoolManager Destroy", DestroyInternal);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		vp_GlobalEventReturn<UnityEngine.Object, Vector3, Quaternion, UnityEngine.Object>.Unregister("vp_PoolManager Instantiate", InstantiateInternal);
		vp_GlobalEvent<UnityEngine.Object, float>.Unregister("vp_PoolManager Destroy", DestroyInternal);
	}

	public virtual void AddObjects(UnityEngine.Object obj, Vector3 position, Quaternion rotation, int amount = 1)
	{
		if (!(obj == null))
		{
			if (!m_AvailableObjects.ContainsKey(obj.name))
			{
				m_AvailableObjects.Add(obj.name, new List<UnityEngine.Object>());
				m_UsedObjects.Add(obj.name, new List<UnityEngine.Object>());
			}
			for (int i = 0; i < amount; i++)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(obj, position, rotation) as GameObject;
				gameObject.name = obj.name;
				gameObject.transform.parent = m_Transform;
				vp_Utility.Activate(gameObject, activate: false);
				m_AvailableObjects[obj.name].Add(gameObject);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual UnityEngine.Object InstantiateInternal(UnityEngine.Object original, Vector3 position, Quaternion rotation)
	{
		if (IgnoredPrefabs.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (GameObject obj) => obj.name == original.name || obj.name == original.name + "(Clone)") != null)
		{
			return UnityEngine.Object.Instantiate(original, position, rotation);
		}
		GameObject gameObject = null;
		List<UnityEngine.Object> value = null;
		List<UnityEngine.Object> value2 = null;
		if (m_AvailableObjects.TryGetValue(original.name, out value))
		{
			while (true)
			{
				m_UsedObjects.TryGetValue(original.name, out value2);
				int num = value.Count + value2.Count;
				if (CustomPrefabs.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (vp_CustomPooledObject obj) => obj.Prefab.name == original.name) == null && num < MaxAmount && value.Count == 0)
				{
					AddObjects(original, position, rotation);
				}
				if (value.Count == 0)
				{
					gameObject = value2.FirstOrDefault() as GameObject;
					if (gameObject == null)
					{
						value2.Remove(gameObject);
						continue;
					}
					vp_Utility.Activate(gameObject, activate: false);
					value2.Remove(gameObject);
					value.Add(gameObject);
				}
				else
				{
					gameObject = value.FirstOrDefault() as GameObject;
					if (!(gameObject == null))
					{
						break;
					}
					value.Remove(gameObject);
				}
			}
			gameObject.transform.position = position;
			gameObject.transform.rotation = rotation;
			value.Remove(gameObject);
			value2.Add(gameObject);
			vp_Utility.Activate(gameObject);
			return gameObject;
		}
		AddObjects(original, position, rotation);
		return InstantiateInternal(original, position, rotation);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DestroyInternal(UnityEngine.Object obj, float t)
	{
		if (obj == null)
		{
			return;
		}
		if (IgnoredPrefabs.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (GameObject o) => o.name == obj.name || o.name == obj.name + "(Clone)") != null || (!m_AvailableObjects.ContainsKey(obj.name) && !PoolOnDestroy))
		{
			UnityEngine.Object.Destroy(obj, t);
			return;
		}
		if (t != 0f)
		{
			vp_Timer.In(t, [PublicizedFrom(EAccessModifier.Internal)] () =>
			{
				DestroyInternal(obj, 0f);
			});
			return;
		}
		if (!m_AvailableObjects.ContainsKey(obj.name))
		{
			AddObjects(obj, Vector3.zero, Quaternion.identity);
			return;
		}
		List<UnityEngine.Object> value = null;
		List<UnityEngine.Object> value2 = null;
		m_AvailableObjects.TryGetValue(obj.name, out value);
		m_UsedObjects.TryGetValue(obj.name, out value2);
		GameObject gameObject = value2.FirstOrDefault([PublicizedFrom(EAccessModifier.Internal)] (UnityEngine.Object o) => o.GetInstanceID() == obj.GetInstanceID()) as GameObject;
		if (!(gameObject == null))
		{
			gameObject.transform.parent = m_Transform;
			vp_Utility.Activate(gameObject, activate: false);
			value2.Remove(gameObject);
			value.Add(gameObject);
		}
	}
}
