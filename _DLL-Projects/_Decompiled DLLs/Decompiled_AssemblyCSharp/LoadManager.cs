using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class LoadManager
{
	public delegate void CompletionCallback();

	public delegate void FileLoadCallback(byte[] _content);

	public class LoadGroup
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LoadGroup parent;

		[PublicizedFrom(EAccessModifier.Private)]
		public int pending;

		public bool Pending => Interlocked.CompareExchange(ref pending, 0, 0) != 0;

		[PublicizedFrom(EAccessModifier.Internal)]
		public LoadGroup(LoadGroup _parent)
		{
			parent = _parent;
			pending = 0;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public void IncrementPending()
		{
			if (!GameManager.IsDedicatedServer)
			{
				for (LoadGroup loadGroup = this; loadGroup != null; loadGroup = loadGroup.parent)
				{
					Interlocked.Increment(ref loadGroup.pending);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public void DecrementPending()
		{
			if (!GameManager.IsDedicatedServer)
			{
				for (LoadGroup loadGroup = this; loadGroup != null; loadGroup = loadGroup.parent)
				{
					Interlocked.Decrement(ref loadGroup.pending);
				}
			}
		}
	}

	public abstract class LoadTask : CustomYieldInstruction
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LoadGroup group;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly bool loadAsync;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool loadStarted;

		public abstract bool IsDone { get; }

		public abstract bool INTERNAL_IsPending { get; }

		public override bool keepWaiting => INTERNAL_IsPending;

		public LoadGroup Group => group;

		[PublicizedFrom(EAccessModifier.Protected)]
		public LoadTask(LoadGroup _group, bool _loadAsync)
		{
			group = _group;
			loadAsync = _loadAsync;
		}

		public abstract bool Load();

		public abstract void LoadSync();

		public virtual void Update()
		{
		}

		public abstract void Complete();
	}

	public class AssetBundleLoadTask : LoadTask
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Action<AssetBundle> callback;

		public override bool INTERNAL_IsPending => true;

		public override bool IsDone => false;

		public AssetBundleLoadTask(LoadGroup _group, bool _loadAsync, Action<AssetBundle> _callback)
			: base(_group, _loadAsync)
		{
			callback = _callback;
		}

		public override bool Load()
		{
			throw new NotImplementedException();
		}

		public override void LoadSync()
		{
			throw new NotImplementedException();
		}

		public override void Complete()
		{
			throw new NotImplementedException();
		}
	}

	public abstract class AssetRequestTask<T> : LoadTask where T : UnityEngine.Object
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Action<T> callback;

		[PublicizedFrom(EAccessModifier.Protected)]
		public T asset;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool assetRetrieved;

		public override bool keepWaiting
		{
			get
			{
				if (!INTERNAL_IsPending)
				{
					return !assetRetrieved;
				}
				return true;
			}
		}

		public override bool IsDone => assetRetrieved;

		public T Asset => asset;

		[PublicizedFrom(EAccessModifier.Protected)]
		public AssetRequestTask(LoadGroup _group, bool _loadAsync, Action<T> _callback)
			: base(_group, _loadAsync)
		{
			callback = _callback;
		}
	}

	public abstract class AssetsRequestTask<T> : LoadTask where T : UnityEngine.Object
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public bool assetsRetrieved;

		public override bool keepWaiting
		{
			get
			{
				if (!INTERNAL_IsPending)
				{
					return !assetsRetrieved;
				}
				return true;
			}
		}

		public override bool IsDone => assetsRetrieved;

		[PublicizedFrom(EAccessModifier.Protected)]
		public AssetsRequestTask(LoadGroup _group, bool _loadAsync)
			: base(_group, _loadAsync)
		{
		}

		public abstract void CollectResults(List<T> _results);
	}

	public class ResourceRequestTask<T> : AssetRequestTask<T> where T : UnityEngine.Object
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string assetPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public ResourceRequest request;

		public override bool INTERNAL_IsPending
		{
			get
			{
				if (loadAsync)
				{
					if (loadStarted)
					{
						if (request != null)
						{
							return !request.isDone;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public ResourceRequestTask(LoadGroup _group, bool _loadAsync, string _assetPath, Action<T> _callback)
			: base(_group, _loadAsync, _callback)
		{
			assetPath = _assetPath;
		}

		public override bool Load()
		{
			request = Resources.LoadAsync<T>(assetPath);
			loadStarted = true;
			return request != null;
		}

		public override void LoadSync()
		{
			asset = Resources.Load<T>(assetPath);
			assetRetrieved = true;
			if (callback != null)
			{
				callback(asset);
			}
		}

		public override void Complete()
		{
			if (INTERNAL_IsPending)
			{
				throw new Exception("ResourceRequestTask still pending.");
			}
			asset = request.asset as T;
			assetRetrieved = true;
			if (callback != null)
			{
				callback(asset);
			}
		}
	}

	public class AssetBundleRequestTask<T> : AssetRequestTask<T> where T : UnityEngine.Object
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly DataLoader.DataPathIdentifier identifier;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isGameObject;

		[PublicizedFrom(EAccessModifier.Private)]
		public AssetBundleManager.AssetBundleRequestTFP request;

		public override bool INTERNAL_IsPending
		{
			get
			{
				if (loadAsync)
				{
					if (loadStarted)
					{
						if (request != null)
						{
							return !request.IsDone;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public AssetBundleRequestTask(LoadGroup _group, bool _loadAsync, DataLoader.DataPathIdentifier _identifier, Action<T> _callback)
			: base(_group, _loadAsync, _callback)
		{
			identifier = _identifier;
			if (typeof(T) == typeof(Transform))
			{
				isGameObject = true;
			}
		}

		public override bool Load()
		{
			if (isGameObject)
			{
				request = AssetBundleManager.Instance.GetAsync<GameObject>(identifier.BundlePath, identifier.AssetName, identifier.FromMod);
			}
			else
			{
				request = AssetBundleManager.Instance.GetAsync<T>(identifier.BundlePath, identifier.AssetName, identifier.FromMod);
			}
			loadStarted = true;
			return request != null;
		}

		public override void LoadSync()
		{
			if (isGameObject)
			{
				GameObject gameObject = AssetBundleManager.Instance.Get<GameObject>(identifier.BundlePath, identifier.AssetName, identifier.FromMod);
				if (gameObject != null)
				{
					asset = gameObject.GetComponent<T>();
				}
			}
			else
			{
				asset = AssetBundleManager.Instance.Get<T>(identifier.BundlePath, identifier.AssetName, identifier.FromMod);
			}
			assetRetrieved = true;
			if (callback != null)
			{
				callback(asset);
			}
		}

		public override void Complete()
		{
			if (INTERNAL_IsPending)
			{
				throw new Exception("AssetBundleRequestTask still pending.");
			}
			if (isGameObject)
			{
				GameObject gameObject = request.Asset as GameObject;
				if (gameObject != null)
				{
					asset = gameObject.GetComponent<T>();
				}
			}
			else
			{
				asset = request.Asset as T;
			}
			assetRetrieved = true;
			if (callback != null)
			{
				callback(asset);
			}
		}
	}

	public class AddressableRequestTask<T> : AssetRequestTask<T> where T : UnityEngine.Object
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object key;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isGameObject;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<T> request;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<GameObject> gameObjectRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly EmptyAddressableRequestTask<T> emptyInstance = new EmptyAddressableRequestTask<T>();

		public override bool INTERNAL_IsPending
		{
			get
			{
				if (loadAsync)
				{
					if (loadStarted && (!request.IsValid() || request.IsDone))
					{
						if (gameObjectRequest.IsValid())
						{
							return !gameObjectRequest.IsDone;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public AddressableRequestTask(LoadGroup _group, bool _loadAsync, object _key, Action<T> _callback)
			: base(_group, _loadAsync, _callback)
		{
			key = _key;
			if (typeof(T) == typeof(Transform))
			{
				isGameObject = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartRequest()
		{
			if (isGameObject)
			{
				gameObjectRequest = Addressables.LoadAssetAsync<GameObject>(key);
			}
			else
			{
				request = Addressables.LoadAssetAsync<T>(key);
			}
			loadStarted = true;
		}

		public override bool Load()
		{
			StartRequest();
			if (!request.IsValid())
			{
				return gameObjectRequest.IsValid();
			}
			return true;
		}

		public override void LoadSync()
		{
			StartRequest();
			if (isGameObject)
			{
				gameObjectRequest.WaitForCompletion();
				GameObject result = gameObjectRequest.Result;
				if (result != null)
				{
					asset = result.GetComponent<T>();
				}
			}
			else
			{
				request.WaitForCompletion();
				asset = request.Result;
			}
			assetRetrieved = true;
			if (callback != null)
			{
				callback(asset);
			}
		}

		public override void Complete()
		{
			if (INTERNAL_IsPending)
			{
				throw new Exception("AssetBundleRequestTask still pending.");
			}
			if (isGameObject)
			{
				GameObject result = gameObjectRequest.Result;
				if (result != null)
				{
					asset = result.GetComponent<T>();
				}
			}
			else
			{
				asset = request.Result;
			}
			assetRetrieved = true;
			if (callback != null)
			{
				callback(asset);
			}
		}

		public void Release()
		{
			asset = null;
			if (isGameObject)
			{
				Addressables.Release(gameObjectRequest);
			}
			else
			{
				Addressables.Release(request);
			}
		}

		public static AddressableRequestTask<T> Empty()
		{
			return emptyInstance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class EmptyAddressableRequestTask<T> : AddressableRequestTask<T> where T : UnityEngine.Object
	{
		public override bool INTERNAL_IsPending => false;

		public override bool IsDone => true;

		public EmptyAddressableRequestTask()
			: base((LoadGroup)null, true, (object)null, (Action<T>)null)
		{
		}

		public override bool Load()
		{
			return false;
		}

		public override void LoadSync()
		{
		}

		public override void Complete()
		{
		}
	}

	public class AddressableAssetsRequestTask<T> : AssetsRequestTask<T> where T : UnityEngine.Object
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public string label;

		[PublicizedFrom(EAccessModifier.Private)]
		public Func<string, bool> addressFilter;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<IList<IResourceLocation>> locationRequest;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool assetRequestStarted;

		[PublicizedFrom(EAccessModifier.Private)]
		public AsyncOperationHandle<IList<T>> assetsRequest;

		public override bool INTERNAL_IsPending
		{
			get
			{
				if (loadAsync)
				{
					if (loadStarted && assetRequestStarted)
					{
						if (assetsRequest.IsValid())
						{
							return !assetsRequest.IsDone;
						}
						return false;
					}
					return true;
				}
				return false;
			}
		}

		public AddressableAssetsRequestTask(LoadGroup _group, bool _loadAsync, string _label, Func<string, bool> _addressFilter = null)
			: base(_group, _loadAsync)
		{
			label = _label;
			addressFilter = _addressFilter;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartLocationsRequest()
		{
			loadStarted = true;
			locationRequest = Addressables.LoadResourceLocationsAsync(label, typeof(T));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void StartAssetsRequest()
		{
			IList<IResourceLocation> list;
			if (addressFilter != null)
			{
				list = new List<IResourceLocation>();
				foreach (IResourceLocation item in locationRequest.Result)
				{
					if (addressFilter(item.PrimaryKey))
					{
						list.Add(item);
					}
				}
			}
			else
			{
				list = locationRequest.Result;
			}
			if (list.Count > 0)
			{
				assetsRequest = Addressables.LoadAssetsAsync<T>(list, null);
			}
			assetRequestStarted = true;
			if (!assetsRequest.IsValid() || assetsRequest.IsDone)
			{
				Complete();
			}
		}

		public override void Update()
		{
			base.Update();
			if (loadAsync && locationRequest.IsValid() && locationRequest.IsDone)
			{
				StartAssetsRequest();
			}
		}

		public override bool Load()
		{
			StartLocationsRequest();
			return locationRequest.IsValid();
		}

		public override void LoadSync()
		{
			StartLocationsRequest();
			if (locationRequest.IsValid())
			{
				locationRequest.WaitForCompletion();
				StartAssetsRequest();
				if (assetsRequest.IsValid())
				{
					assetsRequest.WaitForCompletion();
				}
			}
			Complete();
		}

		public override void Complete()
		{
			if (INTERNAL_IsPending)
			{
				throw new Exception("AssetBundleRequestTask still pending.");
			}
			assetsRetrieved = true;
		}

		public override void CollectResults(List<T> _results)
		{
			if (!assetsRetrieved)
			{
				Log.Warning("Collecting Addressable assets request results before operation has completed");
			}
			if (!assetsRequest.IsValid() || !assetsRetrieved)
			{
				return;
			}
			foreach (T item in assetsRequest.Result)
			{
				_results.Add(item);
			}
		}
	}

	public class CoroutineTask : LoadTask
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IEnumerator task;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly CompletionCallback callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isDone;

		public override bool INTERNAL_IsPending => !isDone;

		public override bool IsDone => !INTERNAL_IsPending;

		public CoroutineTask(LoadGroup _group, IEnumerator _task, CompletionCallback _callback)
			: base(_group, _loadAsync: true)
		{
			task = _task;
			callback = _callback;
		}

		public override bool Load()
		{
			return true;
		}

		public override void LoadSync()
		{
			throw new Exception("CoroutineTask doesn't support synchronous loading.");
		}

		public override void Update()
		{
			isDone = !task.MoveNext();
		}

		public override void Complete()
		{
			if (callback != null)
			{
				callback();
			}
		}
	}

	public class FileLoadTask : LoadTask
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string path;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly FileLoadCallback callback;

		[PublicizedFrom(EAccessModifier.Private)]
		public byte[] content;

		[PublicizedFrom(EAccessModifier.Private)]
		public volatile bool isDone;

		public override bool INTERNAL_IsPending => !isDone;

		public override bool IsDone => !INTERNAL_IsPending;

		public FileLoadTask(LoadGroup _group, bool _loadAsync, string _path, FileLoadCallback _callback)
			: base(_group, _loadAsync)
		{
			path = _path;
			callback = _callback;
		}

		public override bool Load()
		{
			ThreadManager.AddSingleTask([PublicizedFrom(EAccessModifier.Private)] (ThreadManager.TaskInfo _threadInfo) =>
			{
				try
				{
					content = SdFile.ReadAllBytes(path);
				}
				catch (Exception ex)
				{
					Log.Out("LoadManager.FileLoadTask.Load - Failed to load file: " + ex.Message);
					Log.Out(ex.StackTrace);
				}
				isDone = true;
			});
			loadStarted = true;
			return true;
		}

		public override void LoadSync()
		{
			try
			{
				content = SdFile.ReadAllBytes(path);
			}
			catch (Exception ex)
			{
				Log.Out("LoadManager.FileLoadTask.LoadSync - Failed to load file: " + ex.Message);
				Log.Out(ex.StackTrace);
			}
			isDone = true;
			if (callback != null)
			{
				callback(content);
			}
		}

		public override void Complete()
		{
			if (isDone)
			{
				if (callback != null)
				{
					callback(content);
				}
				return;
			}
			throw new Exception("[LoadManager] FileLoadTask still pending.");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Action<LoadTask> updateRequestAction;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string> addressablesCaseMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool forceLoadSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public static LoadGroup rootGroup;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorkBatch<LoadTask> loadRequests;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<LoadTask> deferedLoadRequests;

	public static void Init()
	{
		forceLoadSync = false;
		rootGroup = new LoadGroup(null);
		loadRequests = new WorkBatch<LoadTask>();
		deferedLoadRequests = new List<LoadTask>();
		updateRequestAction = UpdateRequest;
		Addressables.InitializeAsync().WaitForCompletion();
		addressablesCaseMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
		{
			foreach (object key in resourceLocator.Keys)
			{
				if (key is string text && !addressablesCaseMap.TryAdd(text, text))
				{
					Log.Error("Error adding " + text + " to Addressables dictionary, case-insensitive duplicate found.");
				}
			}
		}
	}

	public static void InitSync()
	{
		Init();
		forceLoadSync = true;
	}

	public static void Update()
	{
		lock (deferedLoadRequests)
		{
			int num = loadRequests.Count();
			if (deferedLoadRequests.Count > 0 && num < 64)
			{
				int num2 = Mathf.Min(64 - num, deferedLoadRequests.Count);
				for (int i = 0; i < num2; i++)
				{
					AddTask(deferedLoadRequests[i].Group, deferedLoadRequests[i]);
				}
				deferedLoadRequests.RemoveRange(0, num2);
			}
		}
		loadRequests.DoWork(updateRequestAction);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateRequest(LoadTask _request)
	{
		if (_request.INTERNAL_IsPending)
		{
			_request.Update();
			loadRequests.Add(_request);
		}
		else
		{
			_request.Complete();
			_request.Group.DecrementPending();
		}
	}

	public static void Destroy()
	{
		loadRequests.Clear();
	}

	public static LoadGroup CreateGroup()
	{
		return new LoadGroup(rootGroup);
	}

	public static LoadGroup CreateGroup(LoadGroup _parent)
	{
		return new LoadGroup(_parent);
	}

	public static CoroutineTask AddTask(IEnumerator _task, CompletionCallback _callback = null, LoadGroup _lg = null)
	{
		if (_lg == null)
		{
			_lg = rootGroup;
		}
		_lg.IncrementPending();
		CoroutineTask coroutineTask = new CoroutineTask(_lg, _task, _callback);
		loadRequests.Add(coroutineTask);
		return coroutineTask;
	}

	public static AssetRequestTask<T> LoadAsset<T>(DataLoader.DataPathIdentifier _identifier, Action<T> _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false, bool _ignoreDlcEntitlements = false) where T : UnityEngine.Object
	{
		if (ThreadManager.IsInSyncCoroutine)
		{
			_loadSync = true;
		}
		if (_identifier.IsBundle)
		{
			AssetBundleManager.Instance.LoadAssetBundle(_identifier.BundlePath, _identifier.FromMod);
			return LoadAssetFromBundle(_identifier, _callback, _lg, _deferLoading, _loadSync);
		}
		if (_identifier.Location == DataLoader.DataPathIdentifier.AssetLocation.Addressable)
		{
			return LoadAssetFromAddressables(_identifier.AssetName, _callback, _lg, _deferLoading, _loadSync, _ignoreDlcEntitlements);
		}
		return LoadAssetFromResources(_identifier.AssetName, _callback, _lg, _deferLoading, _loadSync);
	}

	public static AssetRequestTask<T> LoadAsset<T>(string _uri, Action<T> _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : UnityEngine.Object
	{
		return LoadAsset(DataLoader.ParseDataPathIdentifier(_uri), _callback, _lg, _deferLoading, _loadSync);
	}

	public static AssetRequestTask<T> LoadAsset<T>(string _bundlePath, string _assetName, Action<T> _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : UnityEngine.Object
	{
		return LoadAsset(new DataLoader.DataPathIdentifier(_assetName, _bundlePath), _callback, _lg, _deferLoading, _loadSync);
	}

	public static ResourceRequestTask<T> LoadAssetFromResources<T>(string _resourcePath, Action<T> _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : UnityEngine.Object
	{
		if (_lg == null)
		{
			_lg = rootGroup;
		}
		if (ThreadManager.IsInSyncCoroutine && ThreadManager.IsMainThread())
		{
			_loadSync = true;
		}
		ResourceRequestTask<T> resourceRequestTask = new ResourceRequestTask<T>(_lg, !_loadSync, _resourcePath, _callback);
		addOrExecLoadTask(_lg, resourceRequestTask, _deferLoading, _loadSync);
		return resourceRequestTask;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static AssetBundleRequestTask<T> LoadAssetFromBundle<T>(DataLoader.DataPathIdentifier _identifier, Action<T> _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : UnityEngine.Object
	{
		if (_lg == null)
		{
			_lg = rootGroup;
		}
		AssetBundleRequestTask<T> assetBundleRequestTask = new AssetBundleRequestTask<T>(_lg, !_loadSync, _identifier, _callback);
		addOrExecLoadTask(_lg, assetBundleRequestTask, _deferLoading, _loadSync);
		return assetBundleRequestTask;
	}

	public static AddressableRequestTask<T> LoadAssetFromAddressables<T>(object _key, Action<T> _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false, bool _ignoreDlcEntitlements = false) where T : UnityEngine.Object
	{
		if (_lg == null)
		{
			_lg = rootGroup;
		}
		if (!_ignoreDlcEntitlements && !EntitlementManager.Instance.HasEntitlement(_key))
		{
			Log.Error($"Tried to load asset without proper DLC entitlement. Missing {EntitlementManager.Instance.GetSetForAsset(_key)}");
			return AddressableRequestTask<T>.Empty();
		}
		AddressableRequestTask<T> addressableRequestTask = new AddressableRequestTask<T>(_lg, !_loadSync, (_key is string key) ? GetAddressablesCase(key) : _key, _callback);
		addOrExecLoadTask(_lg, addressableRequestTask, _deferLoading, _loadSync);
		return addressableRequestTask;
	}

	public static AddressableRequestTask<T> LoadAssetFromAddressables<T>(string _folderAddress, string _assetPath, Action<T> _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false, bool _ignoreDlcEntitlements = false) where T : UnityEngine.Object
	{
		return LoadAssetFromAddressables(_folderAddress + "/" + _assetPath, _callback, _lg, _deferLoading, _loadSync, _ignoreDlcEntitlements);
	}

	public static AddressableAssetsRequestTask<T> LoadAssetsFromAddressables<T>(string _label, Func<string, bool> _addressFilter = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false) where T : UnityEngine.Object
	{
		if (_lg == null)
		{
			_lg = rootGroup;
		}
		AddressableAssetsRequestTask<T> addressableAssetsRequestTask = new AddressableAssetsRequestTask<T>(_lg, !_loadSync, _label, _addressFilter);
		addOrExecLoadTask(_lg, addressableAssetsRequestTask, _deferLoading, _loadSync);
		return addressableAssetsRequestTask;
	}

	public static void ReleaseAddressable<T>(T _obj)
	{
		Addressables.Release(_obj);
	}

	public static FileLoadTask LoadFile(string _path, FileLoadCallback _callback = null, LoadGroup _lg = null, bool _deferLoading = false, bool _loadSync = false)
	{
		if (_lg == null)
		{
			_lg = rootGroup;
		}
		FileLoadTask fileLoadTask = new FileLoadTask(_lg, !_loadSync, _path, _callback);
		addOrExecLoadTask(_lg, fileLoadTask, _deferLoading, _loadSync);
		return fileLoadTask;
	}

	public static IEnumerator WaitAll(IEnumerable<LoadTask> _tasks)
	{
		foreach (LoadTask task in _tasks)
		{
			while (!task.IsDone)
			{
				yield return null;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addOrExecLoadTask(LoadGroup _lg, LoadTask _task, bool _deferLoading = false, bool _loadSync = false)
	{
		if (!forceLoadSync && !_loadSync)
		{
			_lg.IncrementPending();
			if (_deferLoading)
			{
				lock (deferedLoadRequests)
				{
					deferedLoadRequests.Add(_task);
					return;
				}
			}
			AddTask(_lg, _task);
		}
		else
		{
			_task.LoadSync();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void AddTask(LoadGroup _lg, LoadTask _task)
	{
		if (GameManager.IsDedicatedServer)
		{
			_task.LoadSync();
			_lg.DecrementPending();
		}
		else if (_task.Load())
		{
			loadRequests.Add(_task);
		}
		else
		{
			_lg.DecrementPending();
			_task.Complete();
		}
	}

	public static string GetAddressablesCase(string key)
	{
		if (addressablesCaseMap == null)
		{
			Log.Error("Addressables Case Map not initialised - are you calling GetAddressablesCase before LoadManager.Init?");
		}
		if (string.IsNullOrEmpty(key))
		{
			return key;
		}
		if (addressablesCaseMap.TryGetValue(key, out var value))
		{
			return value;
		}
		return key;
	}
}
