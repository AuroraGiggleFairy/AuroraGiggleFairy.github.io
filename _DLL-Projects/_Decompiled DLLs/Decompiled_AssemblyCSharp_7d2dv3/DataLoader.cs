using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

public static class DataLoader
{
	public struct DataPathIdentifier
	{
		public enum AssetLocation
		{
			Resources,
			Bundle,
			Addressable
		}

		public readonly AssetLocation Location;

		public readonly string BundlePath;

		public readonly string AssetName;

		public readonly bool FromMod;

		public bool IsBundle => Location == AssetLocation.Bundle;

		public DataPathIdentifier(string _assetName, AssetLocation _location = AssetLocation.Resources, bool _fromMod = false)
		{
			BundlePath = null;
			AssetName = _assetName;
			Location = _location;
			FromMod = _fromMod;
		}

		public DataPathIdentifier(string _assetName, string _bundlePath, bool _fromMod = false)
		{
			BundlePath = _bundlePath;
			AssetName = _assetName;
			Location = AssetLocation.Bundle;
			FromMod = _fromMod;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsInResources(string _uri)
	{
		if (_uri.IndexOf('#') < 0)
		{
			return _uri.IndexOf('@') < 0;
		}
		return false;
	}

	public static DataPathIdentifier ParseDataPathIdentifier(string _inputUri)
	{
		if (_inputUri == null)
		{
			return new DataPathIdentifier(null);
		}
		string text = ModManager.PatchModPathString(_inputUri);
		if (text != null)
		{
			_inputUri = text;
		}
		if (_inputUri.IndexOf('#') == 0 && _inputUri.IndexOf('?') > 0)
		{
			int num = _inputUri.IndexOf('?');
			string bundlePath = _inputUri.Substring(1, num - 1);
			_inputUri = _inputUri.Substring(num + 1);
			return new DataPathIdentifier(_inputUri, bundlePath, text != null);
		}
		if (_inputUri.IndexOf("@:") == 0)
		{
			return new DataPathIdentifier(_inputUri.Substring(2), DataPathIdentifier.AssetLocation.Addressable, text != null);
		}
		return new DataPathIdentifier(_inputUri);
	}

	public static T LoadAsset<T>(DataPathIdentifier _identifier, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return LoadManager.LoadAsset<T>(_identifier, null, null, _deferLoading: false, _loadSync: true, _ignoreDlcEntitlements).Asset;
	}

	public static T LoadAsset<T>(string _uri, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return LoadAsset<T>(ParseDataPathIdentifier(_uri), _ignoreDlcEntitlements);
	}

	public static T LoadAsset<T>(AssetReference assetReference, bool _ignoreDlcEntitlements = false) where T : Object
	{
		return LoadManager.LoadAssetFromAddressables<T>(assetReference, null, null, _deferLoading: false, _loadSync: true, _ignoreDlcEntitlements).Asset;
	}

	public static void UnloadAsset(DataPathIdentifier _srcIdentifier, Object _obj)
	{
		if (_srcIdentifier.IsBundle)
		{
			Resources.UnloadUnusedAssets();
			return;
		}
		Resources.UnloadAsset(_obj);
		if (_srcIdentifier.Location == DataPathIdentifier.AssetLocation.Addressable)
		{
			LoadManager.ReleaseAddressable(_obj);
		}
	}

	public static void UnloadAsset(string _uri, Object _obj)
	{
		UnloadAsset(ParseDataPathIdentifier(_uri), _obj);
	}

	public static void PreloadBundle(DataPathIdentifier _identifier)
	{
		if (_identifier.IsBundle)
		{
			AssetBundleManager.Instance.LoadAssetBundle(_identifier.BundlePath, _identifier.FromMod);
		}
	}

	public static void PreloadBundle(string _uri)
	{
		PreloadBundle(ParseDataPathIdentifier(_uri));
	}
}
