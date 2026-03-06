using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class GameObjectPool
{
	public delegate Transform LoadCallback();

	public delegate void CreateCallback(GameObject obj);

	public delegate void CreateAsyncCallback(object _userData, UnityEngine.Object[] _objs, int _objsCount, bool _isAsync);

	public class PoolItem
	{
		public string name;

		public GameObject prefab;

		public LoadCallback loadCallback;

		public CreateCallback createOnceToAllCallback;

		public CreateCallback createCallback;

		public List<GameObject> objs;

		public float updateTime;

		public int activeCount;

		public Color originalTint;

		public GameObject Instantiate()
		{
			activeCount++;
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
			gameObject.name = name;
			if (createOnceToAllCallback != null)
			{
				createOnceToAllCallback(gameObject);
				createOnceToAllCallback = null;
			}
			if (createCallback != null)
			{
				createCallback(gameObject);
			}
			return gameObject;
		}
	}

	public class AsyncItem
	{
		public PoolItem item;

		public CreateAsyncCallback callback;

		public AsyncInstantiateOperation async;

		public object userData;
	}

	public struct ShrinkThreshold(int count, int destroyCount, float delay)
	{
		public int Count = count;

		public int DestroyCount = destroyCount;

		public float Delay = delay;

		public override string ToString()
		{
			return string.Format("({0} = {1}, {2} = {3}, {4} = {5:F2}s)", "Count", Count, "DestroyCount", DestroyCount, "Delay", Delay);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cActivePoolAddAtCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cActivePoolMinCount = 0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cActivePoolRemoveDelay = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObjectPool instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader tintMaskShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, PoolItem> pool = new Dictionary<string, PoolItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PoolItem> activePool = new List<PoolItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<AsyncItem> asyncItems = new List<AsyncItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cAsyncPoolObjsCount = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject[] asyncPoolObjs = new GameObject[128];

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Renderer> tempRenderers = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxPooledInstancesPerItem = 200;

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxDestroysPerUpdate = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ShrinkThreshold shrinkThresholdHigh = new ShrinkThreshold(100, 1, 0.1f);

	[PublicizedFrom(EAccessModifier.Private)]
	public ShrinkThreshold shrinkThresholdMedium = new ShrinkThreshold(40, 1, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public ShrinkThreshold shrinkThresholdLow = new ShrinkThreshold(12, 1, 3f);

	[PublicizedFrom(EAccessModifier.Private)]
	public ShrinkThreshold shrinkThresholdMin = new ShrinkThreshold(0, 1, 10f);

	public static GameObjectPool Instance
	{
		get
		{
			if (instance == null)
			{
				Instantiate();
			}
			return instance;
		}
	}

	public int MaxPooledInstancesPerItem
	{
		get
		{
			return maxPooledInstancesPerItem;
		}
		set
		{
			maxPooledInstancesPerItem = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "MaxPooledInstancesPerItem", maxPooledInstancesPerItem));
		}
	}

	public int MaxDestroysPerUpdate
	{
		get
		{
			return maxDestroysPerUpdate;
		}
		set
		{
			maxDestroysPerUpdate = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "MaxDestroysPerUpdate", maxDestroysPerUpdate));
		}
	}

	public ShrinkThreshold ShrinkThresholdHigh
	{
		get
		{
			return shrinkThresholdHigh;
		}
		set
		{
			shrinkThresholdHigh = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "ShrinkThresholdHigh", shrinkThresholdHigh));
		}
	}

	public ShrinkThreshold ShrinkThresholdMedium
	{
		get
		{
			return shrinkThresholdMedium;
		}
		set
		{
			shrinkThresholdMedium = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "ShrinkThresholdMedium", shrinkThresholdMedium));
		}
	}

	public ShrinkThreshold ShrinkThresholdLow
	{
		get
		{
			return shrinkThresholdLow;
		}
		set
		{
			shrinkThresholdLow = value;
			Log.Out(string.Format("[GameObjectPool] {0} set to {1}", "ShrinkThresholdLow", shrinkThresholdLow));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void Instantiate()
	{
		instance = new GameObjectPool();
	}

	public void Init()
	{
		PlatformOptimizations.ConfigureGameObjectPoolForPlatform(this);
		tintMaskShader = GlobalAssets.FindShader("Game/Entity Tint Mask");
	}

	public void Cleanup()
	{
		foreach (KeyValuePair<string, PoolItem> item in pool)
		{
			List<GameObject> objs = item.Value.objs;
			for (int i = 0; i < objs.Count; i++)
			{
				DestroyObject(objs[i].gameObject);
			}
			objs.Clear();
		}
		activePool.Clear();
		for (int j = 0; j < asyncItems.Count; j++)
		{
			AsyncItem asyncItem = asyncItems[j];
			if (!asyncItem.async.isDone)
			{
				asyncItem.async.Cancel();
			}
		}
		asyncItems.Clear();
	}

	public void FrameUpdate()
	{
		float time = Time.time;
		int num = 0;
		for (int num2 = activePool.Count - 1; num2 >= 0; num2--)
		{
			PoolItem poolItem = activePool[num2];
			if (!(poolItem.updateTime - time > 0f))
			{
				int num3 = poolItem.objs.Count;
				if (num3 <= 0)
				{
					if (poolItem.activeCount <= 0)
					{
						activePool.RemoveAt(num2);
					}
				}
				else
				{
					ShrinkThreshold shrinkThreshold = GetShrinkThreshold(num3);
					poolItem.updateTime = time + shrinkThreshold.Delay;
					int num4 = Mathf.Min(num3, shrinkThreshold.DestroyCount);
					for (int i = 0; i < num4; i++)
					{
						if (num >= maxDestroysPerUpdate)
						{
							break;
						}
						num3--;
						GameObject obj = poolItem.objs[num3];
						poolItem.objs.RemoveAt(num3);
						poolItem.activeCount--;
						DestroyObject(obj);
						num++;
					}
					if (num >= maxDestroysPerUpdate)
					{
						break;
					}
				}
			}
		}
		for (int num5 = asyncItems.Count - 1; num5 >= 0; num5--)
		{
			AsyncItem asyncItem = asyncItems[num5];
			if (asyncItem.async.isDone)
			{
				UnityEngine.Object[] result = asyncItem.async.Result;
				int num6 = result.Length;
				PoolItem item = asyncItem.item;
				item.activeCount += num6;
				for (int j = 0; j < num6; j++)
				{
					GameObject gameObject = (GameObject)result[j];
					gameObject.name = item.name;
					if (item.createOnceToAllCallback != null)
					{
						item.createOnceToAllCallback(gameObject);
						item.createOnceToAllCallback = null;
					}
					if (item.createCallback != null)
					{
						item.createCallback(gameObject);
					}
				}
				asyncItem.callback(asyncItem.userData, result, num6, _isAsync: true);
				asyncItems.RemoveAt(num5);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ShrinkThreshold GetShrinkThreshold(int count)
	{
		if (count >= shrinkThresholdHigh.Count)
		{
			return shrinkThresholdHigh;
		}
		if (count >= shrinkThresholdMedium.Count)
		{
			return shrinkThresholdMedium;
		}
		if (count >= shrinkThresholdLow.Count)
		{
			return shrinkThresholdLow;
		}
		return shrinkThresholdMin;
	}

	public void AddPooledObject(string name, LoadCallback _loadCallback, CreateCallback _createOnceToAllCallback, CreateCallback _createCallback)
	{
		if (!pool.TryGetValue(name, out var value))
		{
			value = new PoolItem();
			value.name = name;
			value.loadCallback = _loadCallback;
			value.createOnceToAllCallback = _createOnceToAllCallback;
			value.createCallback = _createCallback;
			value.objs = new List<GameObject>();
			pool.Add(name, value);
		}
		else
		{
			PoolItem poolItem = value;
			poolItem.createOnceToAllCallback = (CreateCallback)Delegate.Combine(poolItem.createOnceToAllCallback, _createOnceToAllCallback);
		}
		Transform transform = value.loadCallback();
		if ((bool)transform)
		{
			setItemPrefab(value, transform.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setItemPrefab(PoolItem item, GameObject go)
	{
		item.prefab = go;
		getOriginalTint(item, go);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getOriginalTint(PoolItem item, GameObject go)
	{
		bool flag = false;
		List<Color> list = new List<Color>();
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] sharedMaterials = componentsInChildren[i].sharedMaterials;
			foreach (Material material in sharedMaterials)
			{
				if (material != null && material.shader == tintMaskShader)
				{
					list.Add(material.color);
					continue;
				}
				flag = true;
				break;
			}
		}
		item.originalTint = Color.clear;
		if (!flag && list.Count > 0)
		{
			int k;
			for (k = 1; k < list.Count && !(list[0] != list[k]); k++)
			{
			}
			if (k == list.Count)
			{
				item.originalTint = list[0];
			}
		}
	}

	public GameObject GetObjectForType(string objectType)
	{
		Color originalTint;
		return GetObjectForType(objectType, out originalTint);
	}

	public GameObject GetObjectForType(string objectType, out Color originalTint)
	{
		if (!pool.TryGetValue(objectType, out var value))
		{
			Log.Error("GameObjectPool GetObjectForType {0} unknown", objectType);
			originalTint = Color.white;
			return null;
		}
		GameObject gameObject = value.prefab;
		if (!gameObject)
		{
			Transform transform = value.loadCallback();
			if ((bool)transform)
			{
				gameObject = transform.gameObject;
				setItemPrefab(value, gameObject);
			}
		}
		originalTint = value.originalTint;
		if ((bool)gameObject)
		{
			List<GameObject> objs = value.objs;
			int count = objs.Count;
			if (count > 0)
			{
				value.updateTime = Time.time + 5f;
				GameObject result = objs[count - 1];
				objs.RemoveAt(count - 1);
				return result;
			}
			return value.Instantiate();
		}
		return null;
	}

	public AsyncItem GetObjectsForTypeAsync(string objectType, int _count, CreateAsyncCallback _callback, object _userData)
	{
		if (!pool.TryGetValue(objectType, out var value))
		{
			Log.Error("GameObjectPool GetObjectForType {0} unknown", objectType);
			return null;
		}
		GameObject gameObject = value.prefab;
		if (!gameObject)
		{
			Transform transform = value.loadCallback();
			if ((bool)transform)
			{
				gameObject = transform.gameObject;
				setItemPrefab(value, gameObject);
			}
		}
		if (!gameObject)
		{
			return null;
		}
		List<GameObject> objs = value.objs;
		int count = objs.Count;
		if (count >= _count && count <= 128)
		{
			value.updateTime = Time.time + 5f;
			for (int i = 0; i < _count; i++)
			{
				int index = count - 1 - i;
				GameObject gameObject2 = objs[index];
				objs.RemoveAt(index);
				asyncPoolObjs[i] = gameObject2;
			}
			UnityEngine.Object[] objs2 = asyncPoolObjs;
			_callback(_userData, objs2, _count, _isAsync: false);
			return null;
		}
		if (_count <= 3)
		{
			for (int j = 0; j < _count; j++)
			{
				GameObject gameObject3 = value.Instantiate();
				asyncPoolObjs[j] = gameObject3;
			}
			UnityEngine.Object[] objs2 = asyncPoolObjs;
			_callback(_userData, objs2, _count, _isAsync: false);
			return null;
		}
		AsyncItem asyncItem = new AsyncItem();
		asyncItem.item = value;
		asyncItem.callback = _callback;
		asyncItem.userData = _userData;
		asyncItem.async = UnityEngine.Object.InstantiateAsync(gameObject, _count);
		asyncItems.Add(asyncItem);
		return asyncItem;
	}

	public void CancelAsync(AsyncItem _ai)
	{
		if (!asyncItems.Remove(_ai))
		{
			return;
		}
		if (_ai.async.isDone)
		{
			UnityEngine.Object[] result = _ai.async.Result;
			for (int i = 0; i < result.Length; i++)
			{
				UnityEngine.Object.Destroy(result[i]);
			}
		}
		else
		{
			_ai.async.Cancel();
		}
	}

	public void PoolObjectAsync(GameObject obj)
	{
		PoolObject(obj);
	}

	public void PoolObject(GameObject obj)
	{
		if (!obj)
		{
			return;
		}
		string name = obj.name;
		if (!pool.TryGetValue(name, out var value))
		{
			return;
		}
		List<GameObject> objs = value.objs;
		if (objs.Count < maxPooledInstancesPerItem)
		{
			obj.SetActive(value: false);
			obj.transform.SetParent(null, worldPositionStays: false);
			objs.Add(obj);
			if (objs.Count >= 1 && !activePool.Contains(value))
			{
				activePool.Add(value);
			}
		}
		else
		{
			value.activeCount--;
			obj.SetActive(value: false);
			DestroyObject(obj);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyObject(GameObject obj)
	{
		obj.GetComponentsInChildren(tempRenderers);
		Utils.CleanupMaterialsOfRenderers(tempRenderers);
		tempRenderers.Clear();
		UnityEngine.Object.Destroy(obj);
	}

	public void CmdList(string _mode)
	{
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Pool objects:");
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		List<GameObject> list = new List<GameObject>();
		List<GameObject> list2 = new List<GameObject>();
		foreach (KeyValuePair<string, PoolItem> item in pool)
		{
			PoolItem value = item.Value;
			if (value.prefab != null)
			{
				list.Add(value.prefab);
			}
			num += value.activeCount;
			num2 += value.objs.Count;
			if (value.activeCount > 0)
			{
				num3++;
				list2.Add(value.prefab);
			}
			if (_mode == "all" || (_mode == "active" && value.activeCount > 0))
			{
				SingletonMonoBehaviour<SdtdConsole>.Instance.Output(" {0}, prefab {1}, active {2}, count {3}", value.name, value.prefab ? "1" : "0", value.activeCount, value.objs.Count);
			}
		}
		string text = $" types {pool.Count}, used {num3}, pooled {num2}, active {num}";
		if (Application.isEditor)
		{
			ProfilerUtils.CalculateDependentBytes(list.ToArray(), out var meshBytes, out var textureBytes);
			ProfilerUtils.CalculateDependentBytes(list2.ToArray(), out var meshBytes2, out var textureBytes2);
			text += $", used mesh {(double)meshBytes * 9.5367431640625E-07:F2} MB, used texture {(double)textureBytes * 9.5367431640625E-07:F2} MB, required mesh {(double)meshBytes2 * 9.5367431640625E-07:F2} MB, required texture {(double)textureBytes2 * 9.5367431640625E-07:F2} MB";
		}
		SingletonMonoBehaviour<SdtdConsole>.Instance.Output(text);
	}

	public void CmdShrink()
	{
		bool flag;
		do
		{
			flag = false;
			for (int num = activePool.Count - 1; num >= 0; num--)
			{
				PoolItem poolItem = activePool[num];
				if (poolItem.objs.Count > 0)
				{
					poolItem.updateTime = 0f;
					FrameUpdate();
					flag = true;
					break;
				}
			}
		}
		while (flag);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG_GOPOOL_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfilerBegin(string _name)
	{
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[Conditional("DEBUG_GOPOOL_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfilerEnd()
	{
	}
}
