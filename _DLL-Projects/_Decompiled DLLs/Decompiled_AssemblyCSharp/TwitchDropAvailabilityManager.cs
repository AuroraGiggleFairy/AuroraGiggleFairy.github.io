using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Platform;
using UnityEngine;
using UnityEngine.Networking;

public sealed class TwitchDropAvailabilityManager
{
	public sealed class TwitchDropEntry : IEquatable<TwitchDropEntry>
	{
		public readonly string BenefitId;

		public readonly DateTime Start;

		public readonly EntitlementSetEnum EntitlementSet;

		public TwitchDropEntry(string benefitId, DateTime startUtc, EntitlementSetEnum entitlementSet)
		{
			BenefitId = benefitId;
			Start = startUtc;
			EntitlementSet = entitlementSet;
		}

		public bool IsAvailable(DateTime localNow)
		{
			return localNow.ToUniversalTime() >= Start;
		}

		public bool Equals(TwitchDropEntry other)
		{
			if (other == null)
			{
				return false;
			}
			if (this == other)
			{
				return true;
			}
			if (string.Equals(BenefitId, other.BenefitId, StringComparison.OrdinalIgnoreCase))
			{
				DateTime start = Start;
				if (start.Equals(other.Start))
				{
					return EntitlementSet == other.EntitlementSet;
				}
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is TwitchDropEntry other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int num = StringComparer.OrdinalIgnoreCase.GetHashCode(BenefitId ?? string.Empty) * 397;
			DateTime start = Start;
			return ((num ^ start.GetHashCode()) * 397) ^ (int)EntitlementSet;
		}
	}

	public abstract class DropSource
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public const string RfsScheme = "rfs://";

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly TwitchDropAvailabilityManager Owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string OrigUri;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool _isUpdating;

		[PublicizedFrom(EAccessModifier.Protected)]
		public DateTime _lastUpdated;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<TwitchDropEntry> _entries = new List<TwitchDropEntry>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public DropSource(TwitchDropAvailabilityManager owner, string uri)
		{
			Owner = owner;
			OrigUri = uri;
		}

		public void RequestData(bool force)
		{
			if (_isUpdating)
			{
				return;
			}
			DateTime now = DateTime.Now;
			lock (Owner)
			{
				if (!force && _entries.Count > 0 && (now - _lastUpdated).TotalMinutes < 1.0)
				{
					return;
				}
			}
			_lastUpdated = now;
			_isUpdating = true;
			ThreadManager.StartCoroutine(GetDataCo());
		}

		public void GetData(List<TwitchDropEntry> target)
		{
			lock (Owner)
			{
				target.AddRange(_entries);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void ApplyBytes(byte[] bytes)
		{
			_isUpdating = false;
			List<TwitchDropEntry> collection = ParseEntries(bytes);
			lock (Owner)
			{
				_entries.Clear();
				_entries.AddRange(collection);
			}
			Owner.Notify();
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract IEnumerator GetDataCo();

		public static DropSource FromUri(TwitchDropAvailabilityManager owner, string uri)
		{
			if (uri.StartsWith("rfs://", StringComparison.OrdinalIgnoreCase))
			{
				return new DropSourceRfs(owner, uri);
			}
			return new DropSourceWww(owner, uri);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DropSourceRfs : DropSource
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string _rfsKey;

		public DropSourceRfs(TwitchDropAvailabilityManager owner, string uri)
			: base(owner, uri)
		{
			_rfsKey = uri.Substring("rfs://".Length);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				_isUpdating = false;
				yield break;
			}
			if (PlatformManager.NativePlatform.User.UserStatus != EUserStatus.LoggedIn)
			{
				storage.GetCachedFile(_rfsKey, Callback);
				_lastUpdated = DateTime.MinValue;
				yield break;
			}
			bool warned = false;
			float startTime = Time.time;
			while (!storage.IsReady)
			{
				if (storage.Unavailable)
				{
					Log.Warning("Remote Storage is unavailable");
					_isUpdating = false;
					yield break;
				}
				yield return null;
				if (!warned && Time.time > startTime + 30f)
				{
					warned = true;
					Log.Warning("Waiting for drop XML from remote storage exceeded 30s");
				}
			}
			storage.GetFile(_rfsKey, Callback);
			[PublicizedFrom(EAccessModifier.Private)]
			void Callback(IRemoteFileStorage.EFileDownloadResult result, string error, byte[] data)
			{
				if (result != IRemoteFileStorage.EFileDownloadResult.Ok)
				{
					Log.Warning("Retrieving drop XML '" + _rfsKey + "' failed: " + result.ToStringCached() + " (" + error + ")");
					ApplyBytes(null);
				}
				else
				{
					ApplyBytes(data);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class DropSourceWww : DropSource
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string _patchedUri;

		public DropSourceWww(TwitchDropAvailabilityManager owner, string uri)
			: base(owner, uri)
		{
			string text = uri;
			if (!text.StartsWith("http", StringComparison.Ordinal))
			{
				string text2 = ModManager.PatchModPathString(text);
				if (text2 == null)
				{
					throw new ArgumentException("Drop source '" + uri + "' cannot be retrieved: Not http(s) and not '@modfolder:'");
				}
				text = "file://" + text2;
			}
			text = text.Replace("#", "%23").Replace("+", "%2B");
			_patchedUri = text;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			UnityWebRequest req = UnityWebRequest.Get(_patchedUri);
			yield return req.SendWebRequest();
			if (req.result == UnityWebRequest.Result.Success)
			{
				string s = req.downloadHandler.text ?? string.Empty;
				byte[] bytes = Encoding.UTF8.GetBytes(s);
				ApplyBytes(bytes);
			}
			else
			{
				Log.Warning("Retrieving drop XML from '" + OrigUri + "' failed: " + req.error);
				ApplyBytes(null);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TwitchDropAvailabilityManager _instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, DropSource> _sources = new CaseInsensitiveStringDictionary<DropSource>();

	public static TwitchDropAvailabilityManager Instance => _instance ?? (_instance = new TwitchDropAvailabilityManager());

	public event Action<TwitchDropAvailabilityManager> Updated;

	[PublicizedFrom(EAccessModifier.Private)]
	public TwitchDropAvailabilityManager()
	{
	}

	public void RegisterSource(string uri)
	{
		if (!_sources.ContainsKey(uri))
		{
			DropSource dropSource = DropSource.FromUri(this, uri);
			_sources[uri] = dropSource;
			dropSource.RequestData(force: false);
		}
	}

	public void UpdateAll(bool force = false)
	{
		foreach (KeyValuePair<string, DropSource> source in _sources)
		{
			source.Value.RequestData(force);
		}
	}

	public void GetEntries(List<string> sourceUris, List<TwitchDropEntry> target)
	{
		target.Clear();
		foreach (string sourceUri in sourceUris)
		{
			if (_sources.TryGetValue(sourceUri, out var value))
			{
				value.GetData(target);
			}
		}
		target.Sort([PublicizedFrom(EAccessModifier.Internal)] (TwitchDropEntry a, TwitchDropEntry b) =>
		{
			DateTime start = b.Start;
			return start.CompareTo(a.Start);
		});
	}

	public TwitchDropEntry GetLatestForBenefit(string benefitId, List<string> sourceUris)
	{
		List<TwitchDropEntry> list = new List<TwitchDropEntry>();
		GetEntries(sourceUris, list);
		for (int i = 0; i < list.Count; i++)
		{
			if (string.Equals(list[i].BenefitId, benefitId, StringComparison.OrdinalIgnoreCase))
			{
				return list[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void Notify()
	{
		this.Updated?.Invoke(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<TwitchDropEntry> ParseEntries(byte[] bytes)
	{
		List<TwitchDropEntry> list = new List<TwitchDropEntry>();
		if (bytes == null || bytes.Length == 0)
		{
			return list;
		}
		string text = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		XDocument xDocument;
		try
		{
			xDocument = XDocument.Parse(text, LoadOptions.SetLineInfo);
		}
		catch (Exception e)
		{
			Log.Error("TwitchDrop XML parse failed:");
			Log.Exception(e);
			return list;
		}
		XElement root = xDocument.Root;
		if (root == null)
		{
			return list;
		}
		if (!string.Equals(root.Name.LocalName, "drops", StringComparison.OrdinalIgnoreCase))
		{
			IXmlLineInfo xmlLineInfo = root;
			Log.Warning($"TwitchDrop XML unexpected root <{root.Name.LocalName}> at line {xmlLineInfo.LineNumber}, expected <drops>");
			return list;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (XElement item in root.Elements())
		{
			TwitchDropEntry twitchDropEntry = FromXmlBenefit(item);
			if (twitchDropEntry != null)
			{
				if (hashSet.Contains(twitchDropEntry.BenefitId))
				{
					IXmlLineInfo xmlLineInfo2 = item;
					Log.Warning($"TwitchDrop XML duplicate id '{twitchDropEntry.BenefitId}' at line {xmlLineInfo2.LineNumber}; skipping duplicate.");
				}
				else
				{
					hashSet.Add(twitchDropEntry.BenefitId);
					list.Add(twitchDropEntry);
				}
			}
		}
		return list;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static TwitchDropEntry FromXmlBenefit(XElement element)
	{
		if (!string.Equals(element.Name.LocalName, "benefit", StringComparison.OrdinalIgnoreCase))
		{
			Log.Warning($"TwitchDrop XML unknown node <{element.Name.LocalName}> at line {((IXmlLineInfo)element).LineNumber}");
			return null;
		}
		XAttribute xAttribute = element.Attribute("id");
		XAttribute xAttribute2 = element.Attribute("entitlementset");
		XAttribute xAttribute3 = element.Attribute("start");
		if (xAttribute == null || xAttribute3 == null)
		{
			Log.Warning($"TwitchDrop XML missing required attributes on <benefit> at line {((IXmlLineInfo)element).LineNumber} (need id,start)");
			return null;
		}
		string text = (xAttribute.Value ?? string.Empty).Trim();
		if (text.Length == 0)
		{
			Log.Warning($"TwitchDrop XML empty id at line {((IXmlLineInfo)element).LineNumber}");
			return null;
		}
		if (!DateTime.TryParseExact(xAttribute3.Value, "u", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var result))
		{
			Log.Warning($"TwitchDrop XML invalid start '{xAttribute3.Value}' at line {((IXmlLineInfo)element).LineNumber}");
			return null;
		}
		result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
		EntitlementSetEnum entitlementSet = EntitlementSetEnum.None;
		if (xAttribute2 != null)
		{
			if (!Enum.TryParse<EntitlementSetEnum>(xAttribute2.Value.Trim(), ignoreCase: true, out var result2))
			{
				Log.Warning($"TwitchDrop XML invalid entitlementset '{xAttribute2.Value}' for id '{text}' at line {((IXmlLineInfo)element).LineNumber}");
				return null;
			}
			entitlementSet = result2;
		}
		return new TwitchDropEntry(text, result, entitlementSet);
	}
}
