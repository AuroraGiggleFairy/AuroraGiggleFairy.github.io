using System;
using System.Collections.Generic;
using Audio;
using InControl;
using UnityEngine;
using UnityEngine.Scripting;

[UnityEngine.Scripting.Preserve]
public class XUiC_NewsWindow : XUiController
{
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
	public bool hasProviders;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<string> newsProviders = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int maxEntries = int.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoCycleTimePerEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool autoCycle;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction buttonYounger;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerAction buttonOlder;

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
			if (value >= maxEntries)
			{
				value = maxEntries - 1;
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
		hasProviders = newsProviders.Count > 0;
		if (!hasProviders)
		{
			Log.Warning("[XUi] News controller with no sources specified (window group '" + base.WindowGroup.ID + "', window '" + base.ViewComponent.ID + "')");
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
		for (int i = 0; i < maxEntries && i < entries.Count; i++)
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
		RefreshBindings(_forceAll: true);
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
			float num = (float)selector.Value + _dt / autoCycleTimePerEntry;
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
		string url = currentEntry.Url;
		if (url == null || !url.StartsWith("openstore://"))
		{
			XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(base.xui, currentEntry.Url);
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
		autoCycle = autoCycleTimePerEntry > 0f;
		resetIndex();
		RefreshBindings(_forceAll: true);
	}

	public override bool ParseAttribute(string _name, string _value, XUiController _parent)
	{
		switch (_name)
		{
		case "browse_sound":
			base.xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
			{
				browseSound = _o;
			});
			return true;
		case "additional_sources":
		case "sources":
		{
			string[] array = _value.Split(',');
			for (int num = 0; num < array.Length; num++)
			{
				string text = array[num].Trim();
				if (text.Length != 0)
				{
					newsProviders.Add(text);
					NewsManager.Instance.RegisterNewsSource(text);
				}
			}
			return true;
		}
		case "auto_cycle_time_per_entry":
			autoCycleTimePerEntry = StringParsers.ParseFloat(_value);
			return true;
		case "max_entries":
			maxEntries = StringParsers.ParseSInt32(_value);
			return true;
		case "button_younger":
			buttonYounger = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
			if (buttonYounger == null)
			{
				Log.Warning("[XUi] Could not find GUI action '" + _value + "' for news window (window group '" + base.WindowGroup.ID + "', window '" + base.ViewComponent.ID + "')");
			}
			return true;
		case "button_older":
			buttonOlder = base.xui.playerUI.playerInput.GUIActions.GetPlayerActionByName(_value);
			if (buttonOlder == null)
			{
				Log.Warning("[XUi] Could not find GUI action '" + _value + "' for news window (window group '" + base.WindowGroup.ID + "', window '" + base.ViewComponent.ID + "')");
			}
			return true;
		default:
			return base.ParseAttribute(_name, _value, _parent);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool GetBindingValueInternal(ref string _value, string _bindingName)
	{
		NewsManager.NewsEntry newsEntry = CurrentEntry ?? NewsManager.EmptyEntry;
		switch (_bindingName)
		{
		case "has_news_provider":
			_value = hasProviders.ToString();
			return true;
		case "has_news":
			_value = (entries.Count > 0).ToString();
			return true;
		case "news_count":
			_value = Mathf.Min(entries.Count, maxEntries).ToString();
			return true;
		case "is_custom":
			_value = newsEntry.IsCustom.ToString();
			return true;
		case "source":
			_value = newsEntry.CustomListName ?? "";
			return true;
		case "has_text":
			_value = (!string.IsNullOrEmpty(newsEntry.Text)).ToString();
			return true;
		case "headline":
			_value = newsEntry.Headline;
			return true;
		case "headline2":
			_value = newsEntry.Headline2;
			return true;
		case "date":
		{
			DateTime date = newsEntry.Date;
			_value = date.ToString("yyyy-MM-dd");
			return true;
		}
		case "age":
			_value = ValueDisplayFormatters.DateAge(newsEntry.Date);
			return true;
		case "text":
			_value = newsEntry.Text;
			return true;
		case "has_link":
			_value = (!string.IsNullOrEmpty(newsEntry.Url)).ToString();
			return true;
		case "link":
			_value = newsEntry.Url ?? "";
			return true;
		case "has_younger":
			_value = (entries.Count > 0 && CurrentIndex > 0).ToString();
			return true;
		case "has_older":
			_value = (entries.Count > 0 && CurrentIndex < entries.Count - 1).ToString();
			return true;
		default:
			return base.GetBindingValueInternal(ref _value, _bindingName);
		}
	}
}
