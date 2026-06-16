using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AssetBundleManager
{
	public class AssetBundleRequestTFP : CustomYieldInstruction
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Object asset;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AssetBundleRequest request;

		public Object Asset
		{
			get
			{
				if (!IsBundleLoad)
				{
					return asset;
				}
				return request.asset;
			}
		}

		public bool IsDone
		{
			get
			{
				if (IsBundleLoad)
				{
					return request.isDone;
				}
				return true;
			}
		}

		public override bool keepWaiting
		{
			get
			{
				if (IsBundleLoad)
				{
					return !request.isDone;
				}
				return false;
			}
		}

		public bool IsBundleLoad
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return request != null;
			}
		}

		public AssetBundleRequestTFP(Object _asset)
		{
			asset = _asset;
		}

		public AssetBundleRequestTFP(AssetBundleRequest _request)
		{
			request = _request;
		}
	}

	public class AssetBundleMassRequestTFP : CustomYieldInstruction
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Object[] assets;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly AssetBundleRequest request;

		public Object[] Assets
		{
			get
			{
				if (!IsBundleLoad)
				{
					return assets;
				}
				return request.allAssets;
			}
		}

		public bool IsDone
		{
			get
			{
				if (IsBundleLoad)
				{
					return request.isDone;
				}
				return true;
			}
		}

		public override bool keepWaiting
		{
			get
			{
				if (IsBundleLoad)
				{
					return !request.isDone;
				}
				return false;
			}
		}

		public bool IsBundleLoad
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				return request != null;
			}
		}

		public AssetBundleMassRequestTFP(List<Object> _assets)
		{
			assets = _assets.ToArray();
		}

		public AssetBundleMassRequestTFP(AssetBundleRequest _request)
		{
			request = _request;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class AssetBundleRef
	{
		public AssetBundle assetBundle;

		public int version;

		public string url;

		public AssetBundleRef(string _url, int _version)
		{
			url = _url;
			version = _version;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static AssetBundleManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int version = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, AssetBundleRef> dictAssetBundleRefs = new CaseInsensitiveStringDictionary<AssetBundleRef>();

	public static AssetBundleManager Instance => instance ?? (instance = new AssetBundleManager());

	[PublicizedFrom(EAccessModifier.Private)]
	public AssetBundleManager()
	{
	}

	public void LoadAssetBundle(string _name, bool _forceBundle = false)
	{
		string text = ((!Path.IsPathRooted(_name)) ? (GameIO.GetApplicationPath() + "/Data/Bundles/Standalone" + BundleTags.Tag + "/" + _name) : _name);
		string key = _name + 1;
		if (dictAssetBundleRefs.ContainsKey(key))
		{
			return;
		}
		string directoryName = Path.GetDirectoryName(text);
		if (!Directory.Exists(directoryName))
		{
			Log.Error("Loading AssetBundle \"" + text + "\" failed: Parent folder not found!");
			return;
		}
		string fileName = Path.GetFileName(text);
		text = null;
		foreach (string item in Directory.EnumerateFiles(directoryName))
		{
			if (Path.GetFileName(item).EqualsCaseInsensitive(fileName))
			{
				text = item;
				break;
			}
		}
		if (text == null)
		{
			Log.Error("Loading AssetBundle \"" + fileName + "\" failed: File not found!");
			return;
		}
		AssetBundle assetBundle = AssetBundle.LoadFromFile(text);
		if (assetBundle == null)
		{
			Log.Error("Loading AssetBundle \"" + text + "\" failed!");
			return;
		}
		AssetBundleRef assetBundleRef = new AssetBundleRef(text, 1);
		assetBundleRef.assetBundle = assetBundle;
		dictAssetBundleRefs.Add(key, assetBundleRef);
	}

	public T Get<T>(string _bundleName, string _objName, bool _forceBundle = false) where T : Object
	{
		return _get<T>(_bundleName, _objName, _forceBundle);
	}

	public T Get<T>(DataLoader.DataPathIdentifier _dpi, bool _useRelativePath, bool _forceBundle = false) where T : Object
	{
		return _get<T>(_dpi.BundlePath, _dpi.AssetName, _forceBundle, _useRelativePath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T _get<T>(string _bundleName, string _objName, bool _forceBundle = false, bool _useRelativePath = false) where T : Object
	{
		string key = _bundleName + 1;
		if (!dictAssetBundleRefs.TryGetValue(key, out var value))
		{
			return null;
		}
		if (!_useRelativePath)
		{
			if (_objName.IndexOf('/') > 0)
			{
				_objName = _objName.Substring(_objName.LastIndexOf('/') + 1);
			}
			_objName = GameIO.RemoveFileExtension(_objName);
		}
		return value.assetBundle.LoadAsset<T>(_objName);
	}

	public AssetBundleRequestTFP GetAsync<T>(string _bundleName, string _objName, bool _forceBundle = false) where T : Object
	{
		string key = _bundleName + 1;
		if (!dictAssetBundleRefs.TryGetValue(key, out var value))
		{
			return null;
		}
		if (_objName.IndexOf('/') > 0)
		{
			_objName = _objName.Substring(_objName.LastIndexOf('/') + 1);
		}
		return new AssetBundleRequestTFP(value.assetBundle.LoadAssetAsync<T>(GameIO.RemoveFileExtension(_objName)));
	}

	public bool Contains(string _bundleName, string _objName, bool _forceBundle = false)
	{
		string key = _bundleName + 1;
		if (!dictAssetBundleRefs.TryGetValue(key, out var value))
		{
			return false;
		}
		if (_objName.IndexOf('/') > 0)
		{
			_objName = _objName.Substring(_objName.LastIndexOf('/') + 1);
		}
		return value.assetBundle.Contains(GameIO.RemoveFileExtension(_objName));
	}

	public T[] GetAllObjects<T>(string _bundleName, string _subpath = null, bool _forceBundle = false) where T : Object
	{
		string key = _bundleName + 1;
		if (!dictAssetBundleRefs.TryGetValue(key, out var value))
		{
			return null;
		}
		return value.assetBundle.LoadAllAssets<T>();
	}

	public AssetBundleMassRequestTFP GetAllObjectsAsync<T>(string _bundleName, string _subpath = null, bool _forceBundle = false) where T : Object
	{
		string key = _bundleName + 1;
		if (!dictAssetBundleRefs.TryGetValue(key, out var value))
		{
			return null;
		}
		return new AssetBundleMassRequestTFP(value.assetBundle.LoadAllAssetsAsync<T>());
	}

	public string[] GetAllAssetNames(string _bundleName, bool _forceBundle = false)
	{
		string key = _bundleName + 1;
		if (!dictAssetBundleRefs.TryGetValue(key, out var value))
		{
			return null;
		}
		return value.assetBundle.GetAllAssetNames();
	}

	public void Unload(string _name, bool _forceBundle = false)
	{
		string key = _name + 1;
		if (dictAssetBundleRefs.TryGetValue(key, out var value))
		{
			value.assetBundle.Unload(unloadAllLoadedObjects: true);
			value.assetBundle = null;
			dictAssetBundleRefs.Remove(key);
		}
	}

	public void UnloadAll(bool _forceBundle = false)
	{
		foreach (string key in dictAssetBundleRefs.Keys)
		{
			dictAssetBundleRefs[key].assetBundle.Unload(unloadAllLoadedObjects: true);
			dictAssetBundleRefs[key].assetBundle = null;
		}
		dictAssetBundleRefs.Clear();
	}
}
