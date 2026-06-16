using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using Platform;
using UnityEngine;
using UnityEngine.Networking;

public class NewsManager
{
	public abstract class NewsSource
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public const string RemoteFileStorageProtocol = "rfs://";

		public readonly NewsManager Owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string OrigUri;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isUpdating;

		[PublicizedFrom(EAccessModifier.Protected)]
		public DateTime lastUpdated;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<NewsEntry> entries = new List<NewsEntry>();

		public abstract bool IsCustom { get; }

		[PublicizedFrom(EAccessModifier.Protected)]
		public NewsSource(NewsManager _owner, string _uri)
		{
			Owner = _owner;
			OrigUri = _uri;
		}

		public void RequestData(bool _force)
		{
			if (isUpdating)
			{
				return;
			}
			DateTime now = DateTime.Now;
			lock (Owner)
			{
				if (!_force && entries.Count > 0 && (now - lastUpdated).TotalMinutes < 1.0)
				{
					return;
				}
			}
			lastUpdated = now;
			isUpdating = true;
			ThreadManager.StartCoroutine(GetDataCo());
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract IEnumerator GetDataCo();

		public void GetData(List<NewsEntry> _target)
		{
			lock (Owner)
			{
				_target.AddRange(entries);
			}
		}

		public abstract void RequestImage(string _imageRelPath, Action<Texture2D> _callback);

		[PublicizedFrom(EAccessModifier.Protected)]
		public void LoadXml(XmlFile _xml)
		{
			isUpdating = false;
			lock (Owner)
			{
				entries.Clear();
			}
			XElement xElement = _xml?.XmlDoc.Root;
			if (xElement == null)
			{
				Owner.notifyListeners();
				return;
			}
			string text = xElement.GetAttribute("name").Trim();
			if (text == "")
			{
				text = null;
			}
			lock (Owner)
			{
				foreach (XElement item in xElement.Elements("entry"))
				{
					NewsEntry newsEntry = NewsEntry.FromXml(this, text, item);
					if (newsEntry != null)
					{
						entries.Add(newsEntry);
					}
				}
			}
			Owner.notifyListeners();
		}

		public static NewsSource FromUri(NewsManager _owner, string _uri)
		{
			if (_uri.StartsWith("rfs://"))
			{
				return new NewsSourceRfs(_owner, _uri);
			}
			return new NewsSourceWww(_owner, _uri);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class NewsSourceRfs : NewsSource
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string rfsFilename;

		[PublicizedFrom(EAccessModifier.Private)]
		public const int MaxParallelImageRequests = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Queue<(string imageRelPath, Action<Texture2D> callback)> requestedImagesQueue = new Queue<(string, Action<Texture2D>)>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int runningImageRequests;

		public override bool IsCustom => false;

		public NewsSourceRfs(NewsManager _owner, string _uri)
			: base(_owner, _uri)
		{
			rfsFilename = _uri.Substring("rfs://".Length);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				isUpdating = false;
				yield break;
			}
			if (PlatformManager.NativePlatform.User.UserStatus != EUserStatus.LoggedIn)
			{
				storage.GetCachedFile(rfsFilename, fileDownloadedCallback);
				lastUpdated = DateTime.MinValue;
				yield break;
			}
			bool loggedSlow = false;
			float startTime = Time.time;
			while (!storage.IsReady)
			{
				if (storage.Unavailable)
				{
					Log.Warning("Remote Storage is unavailable");
					isUpdating = false;
					yield break;
				}
				yield return null;
				if (!loggedSlow && Time.time > startTime + 30f)
				{
					loggedSlow = true;
					Log.Warning("Waiting for news from remote storage exceeded 30s");
				}
			}
			storage.GetFile(rfsFilename, fileDownloadedCallback);
			[PublicizedFrom(EAccessModifier.Private)]
			void fileDownloadedCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
			{
				if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
				{
					Log.Warning("Retrieving remote news file '" + rfsFilename + "' failed: " + _result.ToStringCached() + " (" + _errorDetails + ")");
					LoadXml(null);
				}
				else
				{
					XmlFile xml = null;
					if (_data != null && _data.Length != 0)
					{
						try
						{
							xml = new XmlFile(_data, _throwExc: true);
						}
						catch (Exception e)
						{
							Log.Error("Failed loading news XML:");
							Log.Exception(e);
							return;
						}
					}
					LoadXml(xml);
				}
			}
		}

		public override void RequestImage(string _imageRelPath, Action<Texture2D> _callback)
		{
			lock (requestedImagesQueue)
			{
				requestedImagesQueue.Enqueue((_imageRelPath, _callback));
			}
			startNextImageRequest();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void startNextImageRequest()
		{
			lock (requestedImagesQueue)
			{
				if (runningImageRequests < 3 && requestedImagesQueue.TryDequeue(out (string, Action<Texture2D>) result))
				{
					ThreadManager.StartCoroutine(requestFromRemoteStorage(result.Item1, result.Item2));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void imageRequestCompleted()
		{
			lock (requestedImagesQueue)
			{
				runningImageRequests--;
				startNextImageRequest();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator requestFromRemoteStorage(string _imageRelPath, Action<Texture2D> _callback)
		{
			IRemoteFileStorage remoteFileStorage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (remoteFileStorage != null)
			{
				lock (requestedImagesQueue)
				{
					runningImageRequests++;
				}
				if (remoteFileStorage.Unavailable)
				{
					remoteFileStorage.GetCachedFile(_imageRelPath, imageDownloadedCallback);
				}
				else
				{
					remoteFileStorage.GetFile(_imageRelPath, imageDownloadedCallback);
				}
			}
			[PublicizedFrom(EAccessModifier.Internal)]
			void imageDownloadedCallback(IRemoteFileStorage.EFileDownloadResult _result, string _errorDetails, byte[] _data)
			{
				imageRequestCompleted();
				if (_result != IRemoteFileStorage.EFileDownloadResult.Ok)
				{
					Log.Warning("Retrieving remote news image file '" + _imageRelPath + "' failed: " + _result.ToStringCached() + " (" + _errorDetails + ")");
				}
				else
				{
					Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGB24, mipChain: false);
					texture2D.LoadImage(_data);
					_callback(texture2D);
				}
			}
			yield break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class NewsSourceWww : NewsSource
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string patchedUri;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string baseUri;

		public override bool IsCustom => true;

		public NewsSourceWww(NewsManager _owner, string _uri)
			: base(_owner, _uri)
		{
			string text = _uri;
			if (!text.StartsWith("http", StringComparison.Ordinal))
			{
				string text2 = ModManager.PatchModPathString(text);
				if (text2 == null)
				{
					throw new ArgumentException("WWW news source '" + _uri + "' can not be retrieved: Neither is a 'http(s)://' URI nor a '@modfolder:' reference.");
				}
				text = "file://" + text2;
			}
			text = (patchedUri = text.Replace("#", "%23").Replace("+", "%2B"));
			int num = text.LastIndexOf('/');
			if (num < 0)
			{
				throw new ArgumentException("WWW news source '" + _uri + "' does not have a valid path");
			}
			baseUri = text.Substring(0, num + 1);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override IEnumerator GetDataCo()
		{
			using UnityWebRequest webRequest = UnityWebRequest.Get(patchedUri);
			yield return webRequest.SendWebRequest();
			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				string text = webRequest.downloadHandler.text;
				XmlFile xml = null;
				if (text != null)
				{
					try
					{
						xml = new XmlFile(text, null, null, _throwExc: true);
					}
					catch (Exception e)
					{
						Log.Error("Failed loading news XML:");
						Log.Exception(e);
						yield break;
					}
				}
				LoadXml(xml);
			}
			else
			{
				Log.Warning("Retrieving custom news file from '" + OrigUri + "' failed: " + webRequest.error);
			}
		}

		public override void RequestImage(string _imageRelPath, Action<Texture2D> _callback)
		{
			ThreadManager.StartCoroutine(requestFromUri(_imageRelPath, _callback));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator requestFromUri(string _imageRelPath, Action<Texture2D> _callback)
		{
			string requestUri = baseUri + _imageRelPath;
			using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(requestUri);
			yield return webRequest.SendWebRequest();
			if (webRequest.result == UnityWebRequest.Result.Success)
			{
				Texture2D content = DownloadHandlerTexture.GetContent(webRequest);
				Texture2D obj = TextureUtils.CloneTexture(content);
				UnityEngine.Object.DestroyImmediate(content);
				_callback(obj);
			}
			else
			{
				Log.Warning("Retrieving custom news image file from '" + requestUri + "' failed: " + webRequest.error);
			}
		}
	}

	public class NewsEntry : IEquatable<NewsEntry>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly NewsSource owner;

		public readonly string CustomListName;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string imageRelPath;

		public readonly string Headline;

		public readonly string Headline2;

		public readonly string Text;

		public readonly string Url;

		public readonly DateTime Date;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool requestedImage;

		[PublicizedFrom(EAccessModifier.Private)]
		public Texture2D image;

		public bool IsCustom
		{
			get
			{
				if (owner != null)
				{
					return owner.IsCustom;
				}
				return true;
			}
		}

		public bool HasImage => !string.IsNullOrEmpty(imageRelPath);

		public bool ImageLoaded => image != null;

		public Texture2D Image => image;

		public NewsEntry(NewsSource _owner, string _customListName, string _imageRelPath, string _headline, string _headline2, string _text, string _url, DateTime _date)
		{
			owner = _owner;
			CustomListName = _customListName;
			imageRelPath = _imageRelPath;
			Headline = _headline;
			Headline2 = _headline2;
			Text = _text;
			Url = _url;
			Date = _date;
		}

		public void RequestImage()
		{
			if (HasImage && !requestedImage)
			{
				requestedImage = true;
				owner.RequestImage(imageRelPath, setImage);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void setImage(Texture2D _image)
		{
			image = _image;
			image.name = "NewsImage_" + imageRelPath;
			image.Compress(highQuality: true);
			image.Apply(updateMipmaps: false, makeNoLongerReadable: true);
			owner.Owner.notifyListeners();
		}

		public bool Equals(NewsEntry _other)
		{
			if (_other == null)
			{
				return false;
			}
			if (this == _other)
			{
				return true;
			}
			if (CustomListName == _other.CustomListName && imageRelPath == _other.imageRelPath && Headline == _other.Headline && Headline2 == _other.Headline2 && Text == _other.Text && Url == _other.Url)
			{
				DateTime date = Date;
				return date.Equals(_other.Date);
			}
			return false;
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (this == _obj)
			{
				return true;
			}
			if (_obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((NewsEntry)_obj);
		}

		public override int GetHashCode()
		{
			int num = ((((((((((((CustomListName != null) ? CustomListName.GetHashCode() : 0) * 397) ^ ((imageRelPath != null) ? imageRelPath.GetHashCode() : 0)) * 397) ^ ((Headline != null) ? Headline.GetHashCode() : 0)) * 397) ^ ((Headline2 != null) ? Headline2.GetHashCode() : 0)) * 397) ^ ((Text != null) ? Text.GetHashCode() : 0)) * 397) ^ ((Url != null) ? Url.GetHashCode() : 0)) * 397;
			DateTime date = Date;
			return num ^ date.GetHashCode();
		}

		public static NewsEntry FromXml(NewsSource _owner, string _customListName, XElement _element)
		{
			string text = null;
			string headline = "";
			string headline2 = "";
			string text2 = "";
			string url = null;
			DateTime result = DateTime.MinValue;
			DateTime result2 = DateTime.MaxValue;
			bool result3 = true;
			foreach (XElement item in _element.Elements())
			{
				switch (item.Name.LocalName)
				{
				case "imagerelpath":
					text = item.Value;
					break;
				case "title":
					headline = item.Value;
					break;
				case "title2":
					headline2 = item.Value;
					break;
				case "text":
					text2 = item.Value;
					break;
				case "link":
					url = item.Value.Trim();
					break;
				case "date":
					if (!DateTime.TryParseExact(item.Value, "u", null, DateTimeStyles.AssumeUniversal, out result))
					{
						Log.Warning($"News XML has an entry with an invalid 'date' element '{item.Value}' at line {((IXmlLineInfo)_element).LineNumber}");
						return null;
					}
					break;
				case "showuntil":
					if (!DateTime.TryParseExact(item.Value, "u", null, DateTimeStyles.AssumeUniversal, out result2))
					{
						Log.Warning($"News XML has an entry with an invalid 'showuntil' element '{item.Value}' at line {((IXmlLineInfo)_element).LineNumber}");
						return null;
					}
					break;
				case "showbefore":
					if (!bool.TryParse(item.Value, out result3))
					{
						Log.Warning($"News XML has an entry with an invalid 'showbefore' element '{item.Value}' at line {((IXmlLineInfo)_element).LineNumber}");
						return null;
					}
					break;
				case "platforms":
				{
					bool flag2 = false;
					string[] array = item.Value.Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						string text4 = array[i].Trim();
						if (!string.IsNullOrEmpty(text4))
						{
							if (!EnumUtils.TryParse<EPlatformIdentifier>(text4, out var _result2, _ignoreCase: true))
							{
								Log.Warning($"News XML has an entry with an invalid 'platforms' element '{item.Value}', platform '{text4}' unknown at line {((IXmlLineInfo)_element).LineNumber}");
								return null;
							}
							if (_result2 == PlatformManager.NativePlatform.PlatformIdentifier || _result2 == PlatformManager.CrossplatformPlatform?.PlatformIdentifier)
							{
								flag2 = true;
								break;
							}
						}
					}
					if (!flag2)
					{
						return null;
					}
					break;
				}
				case "devicetypes":
				{
					bool flag = false;
					string[] array = item.Value.Split(',');
					for (int i = 0; i < array.Length; i++)
					{
						string text3 = array[i].Trim();
						if (!string.IsNullOrEmpty(text3))
						{
							if (!EnumUtils.TryParse<EDeviceType>(text3, out var _result, _ignoreCase: true))
							{
								Log.Warning($"News XML has an entry with an invalid 'devicetypes' element '{item.Value}', devicetype '{text3}' unknown at line {((IXmlLineInfo)_element).LineNumber}");
								return null;
							}
							if (_result == PlatformManager.DeviceType)
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return null;
					}
					break;
				}
				default:
					Log.Warning($"News XML has an entry with an unknown element '{item.Name.LocalName}' at line {((IXmlLineInfo)_element).LineNumber}");
					break;
				}
			}
			if (result == DateTime.MinValue)
			{
				Log.Warning($"News XML has an entry without a date element at line {((IXmlLineInfo)_element).LineNumber}");
				return null;
			}
			DateTime now = DateTime.Now;
			if (!result3 && result > now)
			{
				return null;
			}
			if (result2 < now)
			{
				return null;
			}
			return new NewsEntry(_owner, _customListName, text, headline, headline2, text2, url, result);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static NewsManager instance;

	public static readonly NewsEntry EmptyEntry = new NewsEntry(null, null, null, "- No Entries -", null, "", null, DateTime.Now);

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, NewsSource> sources = new CaseInsensitiveStringDictionary<NewsSource>();

	public static NewsManager Instance => instance ?? (instance = new NewsManager());

	public event Action<NewsManager> Updated;

	public void UpdateNews(bool _force = false)
	{
		foreach (KeyValuePair<string, NewsSource> source in sources)
		{
			source.Deconstruct(out var _, out var value);
			value.RequestData(_force);
		}
	}

	public void RegisterNewsSource(string _uri)
	{
		if (!sources.ContainsKey(_uri))
		{
			NewsSource newsSource = NewsSource.FromUri(this, _uri);
			sources[_uri] = newsSource;
			newsSource.RequestData(_force: false);
		}
	}

	public void GetNewsData(List<string> _sources, List<NewsEntry> _target)
	{
		_target.Clear();
		foreach (string _source in _sources)
		{
			if (sources.TryGetValue(_source, out var value))
			{
				value.GetData(_target);
			}
		}
		sortNewsByAge(_target);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sortNewsByAge(List<NewsEntry> _list)
	{
		_list.Sort([PublicizedFrom(EAccessModifier.Internal)] (NewsEntry _entryA, NewsEntry _entryB) =>
		{
			DateTime date = _entryB.Date;
			return date.CompareTo(_entryA.Date);
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void notifyListeners()
	{
		this.Updated?.Invoke(this);
	}
}
