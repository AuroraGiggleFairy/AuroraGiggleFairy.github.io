using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_LoadingScreen : XUiController
{
	public static string ID = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentTipIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string currentBackground = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showTips = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public AudioClip browseSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiV_Texture backgroundTextureView;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> backgrounds = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> tips = new List<string>();

	[XuiXmlBinding("background_texture")]
	public string CurrentBackground
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentBackground;
		}
	}

	[XuiXmlBinding("show_tips")]
	public bool ShowTips
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return showTips;
		}
	}

	[XuiXmlBinding("count")]
	public int TipCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return tips.Count;
		}
	}

	[XuiXmlBinding("index")]
	public int TipIndex
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return currentTipIndex + 1;
		}
	}

	[XuiXmlBinding("title")]
	public string TipTitle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (currentTipIndex >= 0)
			{
				return Localization.Get(tips[currentTipIndex] + "_title");
			}
			return "";
		}
	}

	[XuiXmlBinding("text")]
	public string TipText
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (currentTipIndex >= 0)
			{
				return Localization.Get(tips[currentTipIndex]);
			}
			return "";
		}
	}

	[XuiXmlAttribute("browse_sound", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public void attribBrowseSound(string _value)
	{
		xui.LoadData(_value, [PublicizedFrom(EAccessModifier.Private)] (AudioClip _o) =>
		{
			browseSound = _o;
		});
	}

	public override void Init()
	{
		base.Init();
		ID = base.WindowGroup.Id;
		XUiController childById = GetChildById("loading_image");
		if (childById != null)
		{
			backgroundTextureView = childById.ViewComponent as XUiV_Texture;
		}
		XUiController childById2 = GetChildById("pnlBlack");
		if (childById2 != null)
		{
			childById2.OnPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				cycle(1);
			};
			childById2.OnRightPress += [PublicizedFrom(EAccessModifier.Private)] (XUiController _, int _) =>
			{
				cycle(-1);
			};
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !XUiC_SpawnSelectionWindow.IsInSpawnSelection(xui.playerUI))
		{
			if (xui.playerUI.playerInput.PermanentActions.PageTipsForward.WasPressed)
			{
				cycle(1);
			}
			else if (xui.playerUI.playerInput.PermanentActions.PageTipsBack.WasPressed)
			{
				cycle(-1);
			}
		}
		if (IsDirty)
		{
			RefreshBindings();
			IsDirty = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void cycle(int _increment)
	{
		if (showTips && !XUiC_VideoPlayer.IsVideoPlaying)
		{
			currentTipIndex += _increment;
			if (currentTipIndex >= tips.Count)
			{
				currentTipIndex = 0;
			}
			else if (currentTipIndex < 0)
			{
				currentTipIndex = tips.Count - 1;
			}
			if (browseSound != null)
			{
				Manager.PlayXUiSound(browseSound, 1f);
			}
			IsDirty = true;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		((XUiV_Window)base.ViewComponent).ForceVisible(1f);
		if (backgrounds.Count > 0)
		{
			Random.InitState(Time.frameCount);
			currentBackground = backgrounds[Random.Range(0, backgrounds.Count)];
		}
		showTips = true;
		currentTipIndex = GamePrefs.GetInt(EnumGamePrefs.LastLoadingTipRead) + 1;
		if (currentTipIndex >= tips.Count)
		{
			currentTipIndex = 0;
		}
		currentTipIndex = Mathf.Clamp(currentTipIndex, 0, tips.Count - 1);
		RefreshBindings();
	}

	public override void OnClose()
	{
		base.OnClose();
		GamePrefs.Set(EnumGamePrefs.LastLoadingTipRead, currentTipIndex);
		backgroundTextureView?.UnloadTexture();
		xui.playerUI.CursorController.SetCursorHidden(_hidden: false);
	}

	public void SetTipsVisible(bool _visible)
	{
		if (showTips != _visible)
		{
			showTips = _visible;
			IsDirty = true;
		}
	}

	public static IEnumerator LoadXml(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		backgrounds.Clear();
		tips.Clear();
		foreach (XElement item in root.Elements())
		{
			if (item.Name == "backgrounds")
			{
				foreach (XElement item2 in item.Elements("tex"))
				{
					if (!item2.HasAttribute("file"))
					{
						Log.Warning("Backgrounds entry is missing file attribute, skipping.");
					}
					else
					{
						backgrounds.Add(item2.GetAttribute("file"));
					}
				}
			}
			else
			{
				if (!(item.Name == "tips"))
				{
					continue;
				}
				foreach (XElement item3 in item.Elements("tip"))
				{
					if (!item3.HasAttribute("key"))
					{
						Log.Warning("Loading tips entry is missing file attribute, skipping.");
					}
					else
					{
						tips.Add(item3.GetAttribute("key"));
					}
				}
			}
		}
		yield break;
	}
}
