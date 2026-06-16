using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewsWindow : XUiController
{
	public delegate void NewsEntryClickedDelegate(NewsManager.NewsEntry _newsEntry);

	[PublicizedFrom(EAccessModifier.Private)]
	public const string PlatformStoreLink = "openstore://";

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip browseSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ComboBoxFloat selector;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture[] bannerTextures;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object newsLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<NewsManager.NewsEntry> entries = new List<NewsManager.NewsEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> newsProviders = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool autoCycle;

	[XuiXmlBinding("has_news_provider")]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public bool HasProviders
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("max_entries", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public int MaxEntries
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	} = int.MaxValue;

	[XuiXmlAttribute("auto_cycle_time_per_entry", false)]
	[field: PublicizedFrom(EAccessModifier.Private)]
	public float AutoCycleTimePerEntry
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int CurrentIndex
	{
		get
		{
			return currentIndex;
		}
		set
		{
			if (value >= entries.Count)
			{
				value = entries.Count - 1;
			}
			if (value >= MaxEntries)
			{
				value = MaxEntries - 1;
			}
			if (value < 0)
			{
				value = 0;
			}
			currentIndex = value;
			IsDirty = true;
		}
	}

	public NewsManager.NewsEntry CurrentEntry
	{
		get
		{
			if (CurrentIndex < 0)
			{
				return null;
			}
			if (CurrentIndex >= entries.Count)
			{
				return null;
			}
			lock (newsLock)
			{
				return entries[CurrentIndex];
			}
		}
	}

	public NewsManager.NewsEntry CurrentEntryOrEmpty => CurrentEntry ?? NewsManager.EmptyEntry;

	[XuiXmlBinding("news_count")]
	public int NewsCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Mathf.Min(entries.Count, MaxEntries);
		}
	}

	[XuiXmlBinding("is_custom")]
	public bool NewsIsCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CurrentEntryOrEmpty.IsCustom;
		}
	}

	[XuiXmlBinding("source")]
	public string NewsSource
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CurrentEntryOrEmpty.CustomListName ?? "";
		}
	}

	[XuiXmlBinding("has_text")]
	public bool NewsHasText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrEmpty(CurrentEntryOrEmpty.Text);
		}
	}

	[XuiXmlBinding("headline")]
	public string NewsHeadline
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CurrentEntryOrEmpty.Headline ?? "";
		}
	}

	[XuiXmlBinding("headline2")]
	public string NewsHeadline2
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CurrentEntryOrEmpty.Headline2 ?? "";
		}
	}

	[XuiXmlBinding("date")]
	public string NewsDate
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			DateTime date = CurrentEntryOrEmpty.Date;
			return date.ToString("yyyy-MM-dd");
		}
	}

	[XuiXmlBinding("age")]
	public string NewsAge
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return ValueDisplayFormatters.DateAge(CurrentEntryOrEmpty.Date);
		}
	}

	[XuiXmlBinding("text")]
	public string NewsText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CurrentEntryOrEmpty.Text ?? "";
		}
	}

	[XuiXmlBinding("has_link")]
	public bool NewsHasLink
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !string.IsNullOrEmpty(CurrentEntryOrEmpty.Url);
		}
	}

	[XuiXmlBinding("link")]
	public string NewsLink
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return CurrentEntryOrEmpty.Url ?? "";
		}
	}

	[XuiXmlBinding("has_younger")]
	public bool HasYounger
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (entries.Count > 0)
			{
				return CurrentIndex > 0;
			}
			return false;
		}
	}

	[XuiXmlBinding("has_older")]
	public bool HasOlder
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (entries.Count > 0)
			{
				return CurrentIndex < entries.Count - 1;
			}
			return false;
		}
	}

	public event NewsEntryClickedDelegate NewsEntryClicked;

	[XuiXmlAttribute("browse_sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribBrowseSound(string _value)
	{
		xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			browseSound = _o;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void parseSourcesString(string _value)
	{
		string[] array = _value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (text.Length != 0)
			{
				newsProviders.Add(text);
				NewsManager.Instance.RegisterNewsSource(text);
			}
		}
	}

	[XuiXmlAttribute("additional_sources", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribAdditionalSources(string _value)
	{
		parseSourcesString(_value);
	}

	[XuiXmlAttribute("sources", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribSources(string _value)
	{
		parseSourcesString(_value);
	}

	public override void Init()
	{
		base.Init();
		XUiController childById = GetChildById("btnYounger");
		if (childById != null)
		{
			childById.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				cycle(-1, _sound: false);
			};
		}
		XUiController childById2 = GetChildById("btnOlder");
		if (childById2 != null)
		{
			childById2.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				cycle(1, _sound: false);
			};
		}
		if (GetChildById("btnLink") is XUiC_SimpleButton xUiC_SimpleButton)
		{
			xUiC_SimpleButton.OnPressed += BtnLink_OnPressed;
		}
		else
		{
			XUiController childById3 = GetChildById("btnLink");
			if (childById3 != null && childById3.ViewComponent is XUiV_Button)
			{
				childById3.OnPress += BtnLink_OnPressed;
				childById3.OnScroll += OnScrollEvent;
			}
		}
		base.OnScroll += OnScrollEvent;
		List<XUiV_Texture> list = new List<XUiV_Texture>();
		XUiController[] childrenById = GetChildrenById("newsImage");
		for (int num = 0; num < childrenById.Length; num++)
		{
			if (childrenById[num]?.ViewComponent is XUiV_Texture item)
			{
				list.Add(item);
			}
		}
		bannerTextures = list.ToArray();
		selector = GetChildByType<XUiC_ComboBoxFloat>();
		if (selector != null)
		{
			selector.OnValueChanged += Selector_OnValueChanged;
		}
		HasProviders = newsProviders.Count > 0;
		if (!HasProviders)
		{
			Log.Warning("[XUi] News controller with no sources specified (window group '" + base.WindowGroup.Id + "', window '" + base.ViewComponent.ID + "')");
		}
		NewsManager.Instance.Updated += newsUpdated;
		newsUpdated(NewsManager.Instance);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnScrollEvent(XUiController _sender, float _delta)
	{
		if (selector == null)
		{
			cycle(Math.Sign(_delta), _sound: true);
		}
		else
		{
			selector.ScrollEvent(_sender, _delta);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Selector_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
	{
		CurrentIndex = Mathf.CeilToInt((float)_newValue) - 1;
		selector.Value = CurrentIndex + 1;
		autoCycle = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newsUpdated(NewsManager _newsManager)
	{
		NewsManager.NewsEntry currentEntry = CurrentEntry;
		lock (newsLock)
		{
			_newsManager.GetNewsData(newsProviders, entries);
		}
		for (int i = 0; i < MaxEntries && i < entries.Count; i++)
		{
			entries[i].RequestImage();
		}
		IsDirty = true;
		if (currentEntry == null || !currentEntry.Equals(CurrentEntry))
		{
			resetIndex();
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		doAutoCycle(_dt);
		if (!IsDirty)
		{
			return;
		}
		RefreshBindings();
		if (bannerTextures != null && bannerTextures.Length != 0)
		{
			Texture2D texture = CurrentEntry?.Image;
			XUiV_Texture[] array = bannerTextures;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Texture = texture;
			}
		}
		IsDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doAutoCycle(float _dt)
	{
		if (selector == null || !autoCycle)
		{
			return;
		}
		NewsManager.NewsEntry currentEntry = CurrentEntry;
		if (currentEntry != null && (!currentEntry.HasImage || currentEntry.ImageLoaded))
		{
			float num = (float)selector.Value + _dt / AutoCycleTimePerEntry;
			if ((double)num > selector.Max)
			{
				num = 0f;
			}
			selector.Value = num;
			CurrentIndex = Mathf.CeilToInt((float)selector.Value) - 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnLink_OnPressed(XUiController _sender, int _mousebutton)
	{
		NewsManager.NewsEntry currentEntry = CurrentEntry;
		if (currentEntry == null)
		{
			return;
		}
		if (this.NewsEntryClicked != null)
		{
			this.NewsEntryClicked(currentEntry);
			return;
		}
		string url = currentEntry.Url;
		if (url == null || !url.StartsWith("openstore://"))
		{
			XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(xui, currentEntry.Url);
			return;
		}
		EntitlementSetEnum entitlementSetEnum;
		try
		{
			entitlementSetEnum = Enum.Parse<EntitlementSetEnum>(currentEntry.Url.Substring("openstore://".Length), ignoreCase: true);
		}
		catch (Exception)
		{
			Log.Error("DLC link uses incorrect value!");
			entitlementSetEnum = EntitlementSetEnum.None;
		}
		if (entitlementSetEnum != EntitlementSetEnum.None)
		{
			EntitlementManager.Instance.OpenStore(entitlementSetEnum, [PublicizedFrom(EAccessModifier.Internal)] (EntitlementSetEnum _) =>
			{
				Log.Out("DLC dialog complete!");
			});
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycle(int _increment, bool _sound)
	{
		lock (newsLock)
		{
			int num = CurrentIndex;
			CurrentIndex += _increment;
			if (CurrentIndex != num && _sound && browseSound != null)
			{
				Manager.PlayXUiSound(browseSound, 1f);
			}
			autoCycle = false;
			if (selector != null)
			{
				selector.Value = CurrentIndex + 1;
			}
			IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetIndex()
	{
		CurrentIndex = 0;
		if (selector != null)
		{
			selector.Value = ((!autoCycle) ? 1 : 0);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		autoCycle = AutoCycleTimePerEntry > 0f;
		resetIndex();
		RefreshBindings();
	}
}
